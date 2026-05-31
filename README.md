# Raijin

Raijin is a .NET Aspire application for combinatorial optimization experiments. It has a small BFF for HTTP entry points, a Combinatorics service split into clean architecture layers, a SAT solver worker, PostgreSQL storage, and a React/Vite SPA served through the BFF.

## Requirements

Install these before running the app:

- Git
- .NET SDK `10.0.100` or newer 10.x feature SDK. The repository pins this in `global.json`.
- Node.js 22 or newer, with npm.
- Docker Desktop, or another Docker-compatible container runtime.
- Aspire CLI.
- Azure CLI, if you plan to deploy to Azure.

Then check the tools:

```powershell
git --version
dotnet --version
node --version
npm --version
docker version
aspire --version
az --version
```

## Download the project

```powershell
git clone git@github.com:vladyslav-pyrch/Raijin.git
cd Raijin
```

If you use HTTPS instead of SSH:

```powershell
git clone https://github.com/vladyslav-pyrch/Raijin.git
cd Raijin
```

Restore backend and frontend dependencies:

```powershell
dotnet restore Raijin.slnx
cd src/spa
npm install
cd ../..
```

The BFF project also runs `npm install` and `npm run build` for the SPA during a .NET build, so the first build can take a little longer.

## Run locally with Aspire

Start Docker first. Aspire uses it for PostgreSQL and the SAT solver container.

From the repository root:

```powershell
aspire start --apphost src/AppHost/Raijin.AppHost.csproj
```

Open the Aspire dashboard URL printed by the command. The main public entry point is the `raijin-bff` resource. Other useful resources are `raijin-comb-api`, `raijin-comb-migrate`, `raijin-comb-sat-solver`, and `raijin-postgres`.

To build everything without starting the app:

```powershell
dotnet build Raijin.slnx
```

## Deploy to Azure with Aspire

The AppHost already describes the Azure target:

- Azure Container Apps environment: `raijin-env`
- Azure PostgreSQL Flexible Server: `raijin-postgres`
- Container App Job for migrations
- Containerized SAT solver
- BFF and Combinatorics API as container apps

Sign in and choose the target subscription:

```powershell
az login
az account set --subscription "<subscription-id>"
az extension add --name containerapp --upgrade
```

Register the Azure providers used by this app:

```powershell
az provider register --namespace Microsoft.App --wait
az provider register --namespace Microsoft.ContainerRegistry --wait
az provider register --namespace Microsoft.DBforPostgreSQL --wait
az provider register --namespace Microsoft.KeyVault --wait
az provider register --namespace Microsoft.OperationalInsights --wait
```

Start Docker, then deploy interactively from the repository root:

```powershell
aspire deploy --apphost src/AppHost/Raijin.AppHost.csproj
```

Aspire will ask for Azure settings, build the app containers, push images, provision the resources, and save deployment state under the AppHost's `.azure` folder.

For a non-interactive deployment, set the Azure values first:

```powershell
$env:Azure__SubscriptionId="<subscription-id>"
$env:Azure__Location="<azure-region>"
$env:Azure__ResourceGroup="<resource-group-name>"
aspire deploy --apphost src/AppHost/Raijin.AppHost.csproj
```

Use a region that supports Azure Container Apps and Azure Database for PostgreSQL Flexible Server. This project has been configured with an Azure PostgreSQL Burstable `Standard_B1ms` SKU and 32 GB of storage in the AppHost.

## Observability

See [docs/observability/logging.md](docs/observability/logging.md) for logging guidance.
