targetScope = 'resourceGroup'

@description('Short deployment environment name, for example prod or stage.')
param environmentName string = 'prod'

@description('Azure region for all regional resources.')
param location string = resourceGroup().location

@secure()
@description('Postgres administrator password used by the postgres container.')
param postgresAdminPassword string

@description('Postgres image to run inside Azure Container Apps.')
param postgresImage string = 'postgres:17.6'

@description('Database name created by the postgres container.')
param databaseName string = 'combinatorics'

@description('Database user created by the postgres container.')
param databaseUser string = 'postgres'

@description('Address space for the application VNet.')
param vnetAddressPrefix string = '10.42.0.0/16'

@description('Subnet used by the public Container Apps environment.')
param publicContainerAppsSubnetPrefix string = '10.42.0.0/23'

@description('Subnet used by the private Container Apps environment.')
param privateContainerAppsSubnetPrefix string = '10.42.2.0/23'

var normalizedEnvironmentName = toLower(environmentName)
var suffix = toLower(uniqueString(resourceGroup().id, normalizedEnvironmentName))
var namePrefix = 'raijin-${normalizedEnvironmentName}'
var workloadProfileName = 'Consumption'

var acrName = take('raijinacr${suffix}', 50)
var containerPullIdentityName = 'mi-${namePrefix}-acr-pull'
var logAnalyticsName = 'law-${namePrefix}-${suffix}'
var vnetName = 'vnet-${namePrefix}'
var publicSubnetName = 'snet-${namePrefix}-public-aca'
var privateSubnetName = 'snet-${namePrefix}-private-aca'
var publicContainerAppsEnvironmentName = 'cae-${namePrefix}-public'
var privateContainerAppsEnvironmentName = 'cae-${namePrefix}-private'
var staticWebAppName = 'stapp-${namePrefix}-${suffix}'
var storageAccountName = take('raijinnfs${suffix}', 24)
var postgresFileShareName = 'postgres-data'
var postgresEnvironmentStorageName = 'postgres-data-nfs'
var postgresContainerAppName = 'ca-${namePrefix}-postgres'

var tags = {
  app: 'raijin'
  environment: normalizedEnvironmentName
}

resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
  name: logAnalyticsName
  location: location
  tags: tags
  properties: {
    retentionInDays: 30
    sku: {
      name: 'PerGB2018'
    }
  }
}

resource vnet 'Microsoft.Network/virtualNetworks@2024-05-01' = {
  name: vnetName
  location: location
  tags: tags
  properties: {
    addressSpace: {
      addressPrefixes: [
        vnetAddressPrefix
      ]
    }
  }
}

resource publicSubnet 'Microsoft.Network/virtualNetworks/subnets@2024-05-01' = {
  parent: vnet
  name: publicSubnetName
  properties: {
    addressPrefix: publicContainerAppsSubnetPrefix
    delegations: [
      {
        name: 'container-apps-delegation'
        properties: {
          serviceName: 'Microsoft.App/environments'
        }
      }
    ]
  }
}

resource privateSubnet 'Microsoft.Network/virtualNetworks/subnets@2024-05-01' = {
  parent: vnet
  name: privateSubnetName
  properties: {
    addressPrefix: privateContainerAppsSubnetPrefix
    serviceEndpoints: [
      {
        service: 'Microsoft.Storage'
      }
    ]
    delegations: [
      {
        name: 'container-apps-delegation'
        properties: {
          serviceName: 'Microsoft.App/environments'
        }
      }
    ]
  }
}

resource publicContainerAppsEnvironment 'Microsoft.App/managedEnvironments@2025-01-01' = {
  name: publicContainerAppsEnvironmentName
  location: location
  tags: tags
  properties: {
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: logAnalytics.properties.customerId
        sharedKey: logAnalytics.listKeys().primarySharedKey
      }
    }
    vnetConfiguration: {
      infrastructureSubnetId: publicSubnet.id
      internal: false
    }
    workloadProfiles: [
      {
        name: workloadProfileName
        workloadProfileType: workloadProfileName
      }
    ]
    zoneRedundant: false
  }
}

resource privateContainerAppsEnvironment 'Microsoft.App/managedEnvironments@2025-01-01' = {
  name: privateContainerAppsEnvironmentName
  location: location
  tags: tags
  properties: {
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: logAnalytics.properties.customerId
        sharedKey: logAnalytics.listKeys().primarySharedKey
      }
    }
    vnetConfiguration: {
      infrastructureSubnetId: privateSubnet.id
      internal: true
    }
    workloadProfiles: [
      {
        name: workloadProfileName
        workloadProfileType: workloadProfileName
      }
    ]
    zoneRedundant: false
  }
}

resource containerRegistry 'Microsoft.ContainerRegistry/registries@2023-07-01' = {
  name: acrName
  location: location
  tags: tags
  sku: {
    name: 'Basic'
  }
  properties: {
    adminUserEnabled: false
  }
}

resource containerPullIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: containerPullIdentityName
  location: location
  tags: tags
}

var acrPullRoleDefinitionId = subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '7f951dda-4ed3-4680-a7ca-43fe172d538d')

resource acrPullRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(containerRegistry.id, containerPullIdentity.id, acrPullRoleDefinitionId)
  scope: containerRegistry
  properties: {
    principalId: containerPullIdentity.properties.principalId
    principalType: 'ServicePrincipal'
    roleDefinitionId: acrPullRoleDefinitionId
  }
}

resource storageAccount 'Microsoft.Storage/storageAccounts@2023-05-01' = {
  name: storageAccountName
  location: location
  tags: tags
  kind: 'FileStorage'
  sku: {
    name: 'Premium_LRS'
  }
  properties: {
    allowBlobPublicAccess: false
    minimumTlsVersion: 'TLS1_2'
    publicNetworkAccess: 'Enabled'
    supportsHttpsTrafficOnly: false
    networkAcls: {
      bypass: 'AzureServices'
      defaultAction: 'Deny'
      virtualNetworkRules: [
        {
          action: 'Allow'
          id: privateSubnet.id
        }
      ]
    }
  }
}

resource fileService 'Microsoft.Storage/storageAccounts/fileServices@2023-05-01' = {
  parent: storageAccount
  name: 'default'
}

resource postgresFileShare 'Microsoft.Storage/storageAccounts/fileServices/shares@2023-05-01' = {
  parent: fileService
  name: postgresFileShareName
  properties: {
    accessTier: 'Premium'
    enabledProtocols: 'NFS'
    rootSquash: 'NoRootSquash'
    shareQuota: 100
  }
}

resource postgresEnvironmentStorage 'Microsoft.App/managedEnvironments/storages@2025-01-01' = {
  parent: privateContainerAppsEnvironment
  name: postgresEnvironmentStorageName
  properties: {
    nfsAzureFile: {
      accessMode: 'ReadWrite'
      server: '${storageAccount.name}.${environment().suffixes.storage}'
      shareName: '/${storageAccount.name}/${postgresFileShare.name}'
    }
  }
}

resource staticWebApp 'Microsoft.Web/staticSites@2025-03-01' = {
  name: staticWebAppName
  location: location
  tags: tags
  sku: {
    name: 'Free'
    tier: 'Free'
  }
  properties: {
    allowConfigFileUpdates: true
    publicNetworkAccess: 'Enabled'
    stagingEnvironmentPolicy: 'Disabled'
  }
}

resource postgresContainerApp 'Microsoft.App/containerApps@2025-01-01' = {
  name: postgresContainerAppName
  location: location
  tags: tags
  properties: {
    environmentId: privateContainerAppsEnvironment.id
    workloadProfileName: workloadProfileName
    configuration: {
      activeRevisionsMode: 'Single'
      ingress: {
        external: true
        exposedPort: 5432
        targetPort: 5432
        transport: 'tcp'
        traffic: [
          {
            latestRevision: true
            weight: 100
          }
        ]
      }
      secrets: [
        {
          name: 'postgres-password'
          value: postgresAdminPassword
        }
      ]
    }
    template: {
      containers: [
        {
          name: 'postgres'
          image: postgresImage
          env: [
            {
              name: 'POSTGRES_DB'
              value: databaseName
            }
            {
              name: 'POSTGRES_USER'
              value: databaseUser
            }
            {
              name: 'POSTGRES_PASSWORD'
              secretRef: 'postgres-password'
            }
            {
              name: 'PGDATA'
              value: '/var/lib/postgresql/data/pgdata'
            }
          ]
          resources: {
            cpu: json('0.5')
            memory: '1Gi'
          }
          volumeMounts: [
            {
              mountPath: '/var/lib/postgresql/data'
              volumeName: 'postgres-data'
            }
          ]
        }
      ]
      scale: {
        minReplicas: 1
        maxReplicas: 1
      }
      volumes: [
        {
          name: 'postgres-data'
          storageName: postgresEnvironmentStorage.name
          storageType: 'NfsAzureFile'
        }
      ]
    }
  }
}

output acrName string = containerRegistry.name
output acrLoginServer string = containerRegistry.properties.loginServer
output containerPullIdentityId string = containerPullIdentity.id
output publicContainerAppsEnvironmentName string = publicContainerAppsEnvironment.name
output privateContainerAppsEnvironmentName string = privateContainerAppsEnvironment.name
output postgresContainerAppName string = postgresContainerApp.name
output postgresHost string = postgresContainerApp.properties.configuration.ingress.fqdn
output staticWebAppName string = staticWebApp.name
output staticWebAppDefaultHostname string = staticWebApp.properties.defaultHostname
output staticWebAppUrl string = 'https://${staticWebApp.properties.defaultHostname}'
