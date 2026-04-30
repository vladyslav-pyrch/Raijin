# Raijin

**Raijin** is a .NET 10 microservices platform for solving combinatorial optimization problems. It exposes a React 19 SPA for end-users and a set of backend services that can run SAT solving, graph, boolean-expression, and pattern-based problem solving jobs.

---

## Architecture Overview

```
┌──────────────────────────────────────────────────────────────┐
│  .NET Aspire AppHost — orchestrates all services & containers │
└──────────────────┬───────────────────────────────────────────┘
                   │
   ┌───────────────┼───────────────────────────────┐
   │               │                               │
   ▼               ▼                               ▼
SPA Frontend   CombinatoricsService API    SAT Solver Worker
(React 19 /    (ASP.NET Core Minimal API)  (Quartz.NET jobs)
  Vite 8)             │
                      ▼
              Migration Worker (EF Core)
                      │
                      ▼
               PostgreSQL (container)
```

### Services

| Service | Description |
|---|---|
| `spa-frontend` | React 19 + TypeScript 5.9 + Vite 8 SPA |
| `combinatorics-service-api` | REST API — CQRS, FluentResults, Minimal APIs |
| `combinatorics-service-sat-solver` | Background optimization jobs via Quartz.NET |
| `combinatorics-service-migration-worker` | EF Core database migrations, runs once on startup |
| `raijin-db-server` | PostgreSQL (managed Docker container, persistent volume) |

---

## Prerequisites

### All platforms

| Requirement | Version | Notes |
|---|---|---|
| [.NET SDK](https://dotnet.microsoft.com/download/dotnet/10.0) | **10.0.x** | Pinned in `global.json` |
| [Node.js](https://nodejs.org/) | **≥ 22.12** (Node 24 LTS recommended) | Required by the Vite SPA |
| [Docker](https://docs.docker.com/get-docker/) | **Engine 27+ / Desktop 4.34+** | Required by Aspire to run PostgreSQL |

> **Note:** Aspire uses Docker to spin up the PostgreSQL container automatically. Docker must be running before you start the AppHost.

### Windows-specific

- [Docker Desktop for Windows](https://docs.docker.com/desktop/install/windows-install/) with WSL 2 backend (recommended) or Hyper-V backend

### Linux-specific

- [Docker Engine](https://docs.docker.com/engine/install/) — Docker Desktop is optional; Engine alone is sufficient  
- Ensure your user is in the `docker` group (no `sudo` needed for Docker commands):
  ```bash
  sudo usermod -aG docker $USER
  newgrp docker
  ```

---

## Setup

### 1. Clone the repository

```bash
git clone https://github.com/vladyslav-pyrch/raijin.git
cd raijin
```

### 2. Install the .NET Aspire workload

```bash
dotnet workload update
dotnet workload install aspire
```

### 3. Restore dependencies

```bash
# .NET packages
dotnet restore Raijin.slnx

# Node packages (SPA)
cd src/spa
npm install
cd ../..
```

### 4. Trust the HTTPS development certificate

**Windows / macOS:**
```bash
dotnet dev-certs https --trust
```

**Linux:**  
The `--trust` flag is not supported on Linux. You must add the cert to your system's trust store manually:

```bash
# Export the certificate
dotnet dev-certs https -ep $HOME/.aspnet/https/dev-cert.pem -np --format Pem

# Debian / Ubuntu
sudo cp $HOME/.aspnet/https/dev-cert.pem /usr/local/share/ca-certificates/aspnet-dev.crt
sudo update-ca-certificates

# RHEL / Fedora
sudo trust anchor --store $HOME/.aspnet/https/dev-cert.pem
sudo update-ca-trust
```

> **Firefox on Linux**: Browser uses its own certificate store. Trust the certificate manually via *Settings → Privacy & Security → View Certificates → Import*.

---

## Running Locally via .NET Aspire

Aspire orchestrates all services (API, SAT Solver, migration worker, PostgreSQL, SPA) in a single command. You do **not** need to start anything manually.

### Windows

```powershell
dotnet run --project src/AppHost/Raijin.AppHost.csproj
```

Or, if you have the [Aspire CLI](https://learn.microsoft.com/dotnet/aspire/fundamentals/aspire-sdk-tooling) installed:

```powershell
aspire run
```

### Linux / macOS

```bash
dotnet run --project src/AppHost/Raijin.AppHost.csproj
```

Or with the Aspire CLI:

```bash
aspire run
```

---

## Accessing the Application

Once the AppHost starts, the **Aspire Dashboard** opens automatically (or navigate to it manually):

| Resource | URL                                                            |
|---|----------------------------------------------------------------|
| **Aspire Dashboard** | `https://localhost:xxxxx`                                      |
| **SPA (frontend)** | printed in Aspire Dashboard under `spa-frontend`               |
| **CombinatoricsService API** | printed in Aspire Dashboard under `combinatorics-service-api`  |
| **Scalar API Reference** | printed in Aspire Dashboard under `scalar-api-reference`       |
| **pgWeb (DB browser)** | printed in Aspire Dashboard under `raijin-db-server` → `pgweb` |

> **Login token**: On first launch, a one-time login URL is printed to the console:  
> `https://localhost:xxxxx/login?t=<token>`  
> Copy and open it in your browser to authenticate into the Dashboard.

---

## Useful Commands

```bash
# Build the entire solution
dotnet build Raijin.slnx

# Run the SPA in standalone dev mode (without Aspire)
cd src/spa
npm run dev

# Lint the SPA
npm run lint

# Build the SPA for production
npm run build
```

---

## Tech Stack

### Backend
- **.NET 10** — ASP.NET Core Minimal APIs
- **.NET Aspire 13** — distributed app orchestration
- **Entity Framework Core** — ORM + migrations (PostgreSQL)
- **MediatR** — CQRS mediator
- **FluentValidation** — input validation
- **FluentResults** — result pattern (no exceptions for flow control)
- **Quartz.NET** — background job scheduling (SAT Solver)

### Frontend
- **React 19** + **TypeScript 5.9**
- **Vite 8** — dev server & bundler
- **Tailwind CSS 4** — utility-first styling
- **React Router 7** — client-side routing

### Infrastructure
- **PostgreSQL** (containerized, managed by Aspire)
- **Docker** — container runtime

