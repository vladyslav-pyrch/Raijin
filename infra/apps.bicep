targetScope = 'resourceGroup'

@description('Short deployment environment name, for example prod or stage.')
param environmentName string = 'prod'

@description('Azure region for app resources.')
param location string = resourceGroup().location

@description('Fully qualified API container image.')
param apiImage string

@description('Fully qualified migration worker container image.')
param migrationWorkerImage string

@description('Fully qualified SAT solver container image.')
param satSolverImage string

@secure()
@description('Postgres administrator password used to build application connection strings.')
param postgresAdminPassword string

@description('Database name created on the PostgreSQL flexible server.')
param databaseName string = 'combinatorics'

@description('Postgres administrator login.')
param databaseUser string = 'raijinadmin'

var normalizedEnvironmentName = toLower(environmentName)
var suffix = toLower(uniqueString(resourceGroup().id, normalizedEnvironmentName))
var namePrefix = 'raijin-${normalizedEnvironmentName}'
var workloadProfileName = 'Consumption'

var acrName = take('raijinacr${suffix}', 50)
var containerPullIdentityName = 'mi-${namePrefix}-acr-pull'
var publicContainerAppsEnvironmentName = 'cae-${namePrefix}-public'
var privateContainerAppsEnvironmentName = 'cae-${namePrefix}-private'
var staticWebAppName = 'stapp-${namePrefix}-${suffix}'
var postgresServerName = 'psql-${namePrefix}-${suffix}'
var apiContainerAppName = 'ca-${namePrefix}-api'
var satSolverContainerAppName = 'ca-${namePrefix}-sat'
var migrationJobName = 'caj-${namePrefix}-migrate'

var databaseConnectionSecretName = 'database-connection-string'
var postgresHost = '${postgresServerName}.postgres.database.azure.com'
var spaOrigin = 'https://${staticWebApp.properties.defaultHostname}'

var tags = {
  app: 'raijin'
  environment: normalizedEnvironmentName
}

resource containerRegistry 'Microsoft.ContainerRegistry/registries@2023-07-01' existing = {
  name: acrName
}

resource containerPullIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' existing = {
  name: containerPullIdentityName
}

resource publicContainerAppsEnvironment 'Microsoft.App/managedEnvironments@2025-01-01' existing = {
  name: publicContainerAppsEnvironmentName
}

resource privateContainerAppsEnvironment 'Microsoft.App/managedEnvironments@2025-01-01' existing = {
  name: privateContainerAppsEnvironmentName
}

resource staticWebApp 'Microsoft.Web/staticSites@2025-03-01' existing = {
  name: staticWebAppName
}

resource apiContainerApp 'Microsoft.App/containerApps@2025-01-01' = {
  name: apiContainerAppName
  location: location
  tags: tags
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${containerPullIdentity.id}': {}
    }
  }
  properties: {
    environmentId: publicContainerAppsEnvironment.id
    workloadProfileName: workloadProfileName
    configuration: {
      activeRevisionsMode: 'Single'
      ingress: {
        allowInsecure: false
        external: true
        targetPort: 8080
        transport: 'http'
        traffic: [
          {
            latestRevision: true
            weight: 100
          }
        ]
      }
      registries: [
        {
          server: containerRegistry.properties.loginServer
          identity: containerPullIdentity.id
        }
      ]
      secrets: [
        {
          name: databaseConnectionSecretName
          #disable-next-line use-secure-value-for-secure-inputs // Connection string includes the secure postgresAdminPassword parameter and is stored as a Container Apps secret.
          value: 'Host=${postgresHost};Port=5432;Database=${databaseName};Username=${databaseUser};Password=${postgresAdminPassword};Ssl Mode=Require'
        }
      ]
    }
    template: {
      containers: [
        {
          name: 'api'
          image: apiImage
          env: [
            {
              name: 'ASPNETCORE_ENVIRONMENT'
              value: 'Production'
            }
            {
              name: 'ASPNETCORE_FORWARDEDHEADERS_ENABLED'
              value: 'true'
            }
            {
              name: 'ASPNETCORE_HTTP_PORTS'
              value: '8080'
            }
            {
              name: 'COMBINATORICS_DB_CONNECTION_STRING'
              secretRef: databaseConnectionSecretName
            }
            {
              name: 'Cors__AllowedOrigins__0'
              value: spaOrigin
            }
          ]
          probes: [
            {
              type: 'Liveness'
              initialDelaySeconds: 10
              periodSeconds: 30
              timeoutSeconds: 5
              httpGet: {
                path: '/alive'
                port: 8080
                scheme: 'HTTP'
              }
            }
            {
              type: 'Readiness'
              initialDelaySeconds: 10
              periodSeconds: 10
              timeoutSeconds: 5
              httpGet: {
                path: '/health'
                port: 8080
                scheme: 'HTTP'
              }
            }
          ]
          resources: {
            cpu: json('0.5')
            memory: '1Gi'
          }
        }
      ]
      scale: {
        minReplicas: 1
        maxReplicas: 3
      }
    }
  }
}

resource satSolverContainerApp 'Microsoft.App/containerApps@2025-01-01' = {
  name: satSolverContainerAppName
  location: location
  tags: tags
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${containerPullIdentity.id}': {}
    }
  }
  properties: {
    environmentId: privateContainerAppsEnvironment.id
    workloadProfileName: workloadProfileName
    configuration: {
      activeRevisionsMode: 'Single'
      registries: [
        {
          server: containerRegistry.properties.loginServer
          identity: containerPullIdentity.id
        }
      ]
      secrets: [
        {
          name: databaseConnectionSecretName
          #disable-next-line use-secure-value-for-secure-inputs // Connection string includes the secure postgresAdminPassword parameter and is stored as a Container Apps secret.
          value: 'Host=${postgresHost};Port=5432;Database=${databaseName};Username=${databaseUser};Password=${postgresAdminPassword};Ssl Mode=Require'
        }
      ]
    }
    template: {
      containers: [
        {
          name: 'sat-solver'
          image: satSolverImage
          env: [
            {
              name: 'DOTNET_ENVIRONMENT'
              value: 'Production'
            }
            {
              name: 'COMBINATORICS_DB_CONNECTION_STRING'
              secretRef: databaseConnectionSecretName
            }
            {
              name: 'MAX_JOBS_COUNT'
              value: '3'
            }
            {
              name: 'MAX_REFIRE_COUNT'
              value: '3'
            }
          ]
          resources: {
            cpu: json('1.0')
            memory: '2Gi'
          }
        }
      ]
      scale: {
        minReplicas: 1
        maxReplicas: 1
      }
    }
  }
}

resource migrationWorkerJob 'Microsoft.App/jobs@2025-01-01' = {
  name: migrationJobName
  location: location
  tags: tags
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${containerPullIdentity.id}': {}
    }
  }
  properties: {
    environmentId: privateContainerAppsEnvironment.id
    workloadProfileName: workloadProfileName
    configuration: {
      manualTriggerConfig: {
        parallelism: 1
        replicaCompletionCount: 1
      }
      registries: [
        {
          server: containerRegistry.properties.loginServer
          identity: containerPullIdentity.id
        }
      ]
      replicaRetryLimit: 3
      replicaTimeout: 1800
      secrets: [
        {
          name: databaseConnectionSecretName
          #disable-next-line use-secure-value-for-secure-inputs // Connection string includes the secure postgresAdminPassword parameter and is stored as a Container Apps secret.
          value: 'Host=${postgresHost};Port=5432;Database=${databaseName};Username=${databaseUser};Password=${postgresAdminPassword};Ssl Mode=Require'
        }
      ]
      triggerType: 'Manual'
    }
    template: {
      containers: [
        {
          name: 'migration-worker'
          image: migrationWorkerImage
          env: [
            {
              name: 'DOTNET_ENVIRONMENT'
              value: 'Production'
            }
            {
              name: 'COMBINATORICS_DB_CONNECTION_STRING'
              secretRef: databaseConnectionSecretName
            }
          ]
          resources: {
            cpu: json('0.5')
            memory: '1Gi'
          }
        }
      ]
    }
  }
}

output apiContainerAppName string = apiContainerApp.name
output apiUrl string = 'https://${apiContainerApp.properties.configuration.ingress.fqdn}'
output satSolverContainerAppName string = satSolverContainerApp.name
output migrationJobName string = migrationWorkerJob.name
output staticWebAppName string = staticWebApp.name
output staticWebAppUrl string = spaOrigin
