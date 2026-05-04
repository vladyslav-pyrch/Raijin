targetScope = 'resourceGroup'

@description('Short deployment environment name, for example prod or stage.')
param environmentName string = 'prod'

@description('Azure region for all regional resources.')
param location string = resourceGroup().location

@secure()
@description('Postgres administrator password used by Azure Database for PostgreSQL Flexible Server.')
param postgresAdminPassword string

@description('Postgres major version for Azure Database for PostgreSQL Flexible Server.')
param postgresVersion string = '17'

@description('Database name created on the PostgreSQL flexible server.')
param databaseName string = 'combinatorics'

@description('Postgres administrator login.')
param databaseUser string = 'raijinadmin'

@description('Cheapest intended PostgreSQL flexible server SKU.')
param postgresSkuName string = 'Standard_B1ms'

@description('PostgreSQL flexible server SKU tier.')
param postgresSkuTier string = 'Burstable'

@description('PostgreSQL flexible server storage size in GiB. 32 GiB is the minimum flexible server storage size.')
param postgresStorageSizeGB int = 32

@description('Address space for the application VNet.')
param vnetAddressPrefix string = '10.42.0.0/16'

@description('Subnet used by the public Container Apps environment.')
param publicContainerAppsSubnetPrefix string = '10.42.0.0/23'

@description('Subnet used by the private Container Apps environment.')
param privateContainerAppsSubnetPrefix string = '10.42.2.0/23'

@description('Subnet used by Azure Database for PostgreSQL Flexible Server.')
param postgresSubnetPrefix string = '10.42.4.0/27'

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
var postgresSubnetName = 'snet-${namePrefix}-postgres'
var publicContainerAppsEnvironmentName = 'cae-${namePrefix}-public'
var privateContainerAppsEnvironmentName = 'cae-${namePrefix}-private'
var staticWebAppName = 'stapp-${namePrefix}-${suffix}'
var postgresServerName = 'psql-${namePrefix}-${suffix}'
var postgresPrivateDnsZoneName = 'private-${namePrefix}-${suffix}.postgres.database.azure.com'

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

resource postgresSubnet 'Microsoft.Network/virtualNetworks/subnets@2024-05-01' = {
  parent: vnet
  name: postgresSubnetName
  properties: {
    addressPrefix: postgresSubnetPrefix
    delegations: [
      {
        name: 'postgres-flexible-server-delegation'
        properties: {
          serviceName: 'Microsoft.DBforPostgreSQL/flexibleServers'
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

resource postgresPrivateDnsZone 'Microsoft.Network/privateDnsZones@2024-06-01' = {
  name: postgresPrivateDnsZoneName
  location: 'global'
  tags: tags
}

resource postgresPrivateDnsZoneVnetLink 'Microsoft.Network/privateDnsZones/virtualNetworkLinks@2024-06-01' = {
  parent: postgresPrivateDnsZone
  name: '${vnetName}-link'
  location: 'global'
  tags: tags
  properties: {
    registrationEnabled: false
    virtualNetwork: {
      id: vnet.id
    }
  }
}

resource postgresServer 'Microsoft.DBforPostgreSQL/flexibleServers@2024-08-01' = {
  name: postgresServerName
  location: location
  tags: tags
  sku: {
    name: postgresSkuName
    tier: postgresSkuTier
  }
  properties: {
    administratorLogin: databaseUser
    administratorLoginPassword: postgresAdminPassword
    authConfig: {
      activeDirectoryAuth: 'Disabled'
      passwordAuth: 'Enabled'
    }
    backup: {
      backupRetentionDays: 7
      geoRedundantBackup: 'Disabled'
    }
    highAvailability: {
      mode: 'Disabled'
    }
    network: {
      delegatedSubnetResourceId: postgresSubnet.id
      privateDnsZoneArmResourceId: postgresPrivateDnsZone.id
    }
    storage: {
      autoGrow: 'Disabled'
      storageSizeGB: postgresStorageSizeGB
    }
    version: postgresVersion
  }
  dependsOn: [
    postgresPrivateDnsZoneVnetLink
  ]
}

resource postgresDatabase 'Microsoft.DBforPostgreSQL/flexibleServers/databases@2024-08-01' = {
  parent: postgresServer
  name: databaseName
  properties: {
    charset: 'UTF8'
    collation: 'en_US.utf8'
  }
}

output acrName string = containerRegistry.name
output acrLoginServer string = containerRegistry.properties.loginServer
output containerPullIdentityId string = containerPullIdentity.id
output publicContainerAppsEnvironmentName string = publicContainerAppsEnvironment.name
output privateContainerAppsEnvironmentName string = privateContainerAppsEnvironment.name
output postgresServerName string = postgresServer.name
output postgresHost string = postgresServer.properties.fullyQualifiedDomainName
output staticWebAppName string = staticWebApp.name
output staticWebAppDefaultHostname string = staticWebApp.properties.defaultHostname
output staticWebAppUrl string = 'https://${staticWebApp.properties.defaultHostname}'
