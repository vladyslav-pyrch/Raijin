# Azure CI/CD Deployment

This repo deploys Raijin with GitHub Actions, Bicep, Azure Container Apps, Azure Container Registry, and Azure Static Web Apps.

## Topology

- `postgres:17.6` runs as a Container App in an internal Container Apps environment on the private subnet. Its TCP endpoint is reachable from the VNet, not from the public internet.
- `CombinatoricsService.MigrationWorker` runs as a manual Container App Job in the same private environment and applies EF migrations.
- `CombinatoricsService.SatSolver` runs as a private Container App in the same private environment.
- `CombinatoricsService.Api` runs as a public Container App in a second Container Apps environment on the public subnet.
- `spa` is deployed to Azure Static Web Apps using the default generated hostname.
- No custom domains are configured.

The database container uses an Azure Files share for `/var/lib/postgresql/data`. This keeps the cost profile low while avoiding a fully ephemeral database container. It is still not a substitute for a managed production database.

## GitHub Configuration

Create a GitHub environment named `azure`, then configure these repository variables:

| Name | Purpose |
| --- | --- |
| `AZURE_CLIENT_ID` | Client ID of the federated Azure app registration/service principal. |
| `AZURE_TENANT_ID` | Azure tenant ID. |
| `AZURE_SUBSCRIPTION_ID` | Azure subscription ID. |
| `AZURE_LOCATION` | Optional. Defaults to `westeurope`. |
| `AZURE_RESOURCE_GROUP_NAME` | Optional. Defaults to `rg-raijin-<environment>`. |

Configure this repository secret:

| Name | Purpose |
| --- | --- |
| `POSTGRES_PASSWORD` | Password injected into the postgres container and backend connection strings. |

The Azure identity used by GitHub Actions needs enough permission to create the resource group, deploy resources, and create the ACR pull role assignment. The simplest setup is `Owner` at subscription scope. A narrower setup is `Contributor` plus `User Access Administrator` for the target subscription or resource group scope.

## Workflow

The workflow is `.github/workflows/azure-ci-cd.yml`.

- Pull requests run validation only: .NET restore/build and SPA install/build.
- Pushes to `main` and manual runs deploy to Azure.
- Deployment creates or updates core infrastructure from `infra/core.bicep`.
- The workflow builds and pushes the API, migration worker, and SAT solver images to ACR.
- App resources are created or updated from `infra/apps.bicep`.
- The migration job is started and polled until it succeeds or fails.
- The SPA is built with `VITE_COMBINATORICS_API_URL` set to the generated API URL, then deployed to Static Web Apps.

Manual runs can override:

- `environment_name`: resource-name suffix, default `prod`.
- `location`: Azure region, default `westeurope`.

## Infrastructure Files

- `infra/core.bicep`: VNet, public/private subnets, Container Apps environments, ACR, managed identity, role assignment, storage, Static Web App, and postgres Container App.
- `infra/apps.bicep`: API Container App, SAT solver Container App, and migration Container App Job.

The API receives `Cors__AllowedOrigins__0` from the generated Static Web Apps hostname. The SPA receives `VITE_COMBINATORICS_API_URL` from the generated API hostname. Both use Azure-generated default domains.
