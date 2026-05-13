# AGENTS.md

## Raijin AI Agent Guide

This document provides essential, actionable knowledge for AI coding agents to be productive in the Raijin codebase. It summarizes architecture, workflows, conventions, and integration points specific to this project.

---

### 1. Big Picture Architecture

- **Raijin** is a .NET 10 microservices platform for combinatorial optimization, orchestrated with .NET Aspire.
- **Major services:**
  - `Bff`: Entry point for HTTP traffic, minimal logic.
  - `CombinatoricsService` has `Api/`, `Application/`, `Domain/`, `Infrastructure/`, and `MigrationWorker/` `SatSolver/` subfolders, following Clean Architecture.
  - `AppHost`: Orchestration/hosting logic.
  - `spa/`: React 19 + TypeScript frontend, built with Vite.
- **Data flow:**
  - Bff routes requests to backend services.
  - Services use CQRS (commands/queries) and vertical slice features.
  - Data persistence via PostgreSQL (see Infrastructure layer).

---

### 2. Critical Developer Workflows

- **Backend:**
  - Build all: `dotnet build Raijin.slnx`
  - Run all services: orchestrated via .NET Aspire (see `AppHost/`)
  - Add feature: create vertical slice in `Application/` and `Api/Endpoints/` (see below)
- **Frontend:**
  - Dev server: `cd spa && npm install && npm run dev`
  - Build: `npm run build`
  - Lint: `npm run lint` (see `eslint.config.js`)
- **Testing:**
  - (Add test commands here if/when present)

---

### 3. Project-Specific Conventions & Patterns

- **Clean Architecture:**
  - Four layers: Api → Application → Infrastructure → Domain (dependencies point inward)
  - No cross-layer dependencies except as allowed by Clean Architecture
- **CQRS & Vertical Slices:**
  - Each feature (e.g., `CreateProblem`) is a folder/file grouping: Command, Handler, Validator, Result
  - Example: `src/CombinatoricsService/Application/Problems/CreateProblemCommand.cs`
- **Result Pattern:**
  - Use [FluentResults](https://github.com/altmann/FluentResults) for all command/query outcomes (no exceptions for flow control)
  - Example:

    ```csharp
    return Result.Ok(new MyResult(id));
    return Result.Fail(new ValidationError("field", "message"));
    if (result.IsFailed) return TypedResults.BadRequest(result.ToProblemDetails());
    ```

- **Frontend:**
  - React 19, TypeScript 5.9, Vite 8
  - Use functional components, custom hooks, and CSS modules
  - TypeScript config: see `spa/tsconfig.*.json`

---

### 4. Integration Points & External Dependencies

- **Database:** PostgreSQL (configured in Infrastructure layer)
- **Validation:** FluentValidation (backend)
- **Background jobs:** Quartz.NET
- **Orchestration:** .NET Aspire (`AppHost/`)
- **API communication:** Minimal APIs, MediatR for CQRS
- **Frontend-backend:** REST via Bff

---

### 5. Key Files & Directories

- `src/CombinatoricsService/Application/` — Application layer, CQRS features
- `src/CombinatoricsService/Api/Endpoints/` — HTTP endpoints
- `src/CombinatoricsService/Domain/` — Business logic
- `src/CombinatoricsService/Infrastructure/` — Data access, external integrations
- `spa/src/` — React app source

---
