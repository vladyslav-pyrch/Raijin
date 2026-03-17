<!-- Why this file exists: Deep architectural reference for developers and Copilot.
     Explains the WHY behind design decisions, layer responsibilities, data flow, and anti-patterns. -->

# Raijin — Architecture Reference

## 1. System Context

Raijin is a platform for solving combinatorial optimisation problems. Users define decision variables with discrete states and constraints expressed as boolean formulas. The system reduces these to SAT (Boolean satisfiability) problems, solves them using the CryptoMiniSat engine, and returns solutions.

```
┌────────┐       ┌────────────┐       ┌──────────────────────┐       ┌──────────────┐
│  User  │──────►│ Angular SPA│──────►│     API Gateway      │──────►│  Services    │
│Browser │◄──────│  (rjn-)    │◄──────│ (auth, CORS, rate)   │◄──────│              │
└────────┘       └────────────┘       └──────────────────────┘       └──────┬───────┘
                                                                            │
                                                                    ┌───────▼───────┐
                                                                    │   RabbitMQ    │
                                                                    │ (MassTransit) │
                                                                    └───────┬───────┘
                                                                            │
                                                                    ┌───────▼───────┐
                                                                    │   Workers /   │
                                                                    │   Consumers   │
                                                                    └───────────────┘
```

### Services

| Service | Responsibility |
|---|---|
| **CombinatoricsService** | Accepts combinatoric problem definitions, validates them, converts to SAT via Tseitin transform, publishes SAT problem for solving |
| **SatSolver** | Accepts raw DIMACS SAT problems, delegates to CryptoMiniSat, publishes solutions |
| **IdentityService** | User authentication and authorization |
| **QueryService** | Read-model projections (CQRS query side), Redis caching |
| **ApiGateway** | Edge concerns — CORS, rate limiting, HTTPS redirect, auth verification, reverse proxy |

### Infrastructure

| Component | Purpose |
|---|---|
| **PostgreSQL** | One database per service (physical isolation) |
| **RabbitMQ** | Asynchronous inter-service messaging |
| **Redis** | Query service caching layer |
| **.NET Aspire** | Local orchestration, service discovery, health checks, OpenTelemetry |

## 2. Clean Architecture — Layer Responsibilities

Each microservice follows strict Clean Architecture. The dependency rule is enforced at the project reference level:

```
Api → Infrastructure → Application → Domain
                         ↑
                     Contracts (shared integration event interfaces only)
```

### Domain Layer

- **Owns**: Aggregate roots, entities, value objects, domain services, domain exceptions.
- **Rules**:
  - Zero NuGet package references. Only `Microsoft.NET.Sdk`.
  - No `using` of anything outside the Domain namespace (except `System.*`).
  - All invariants enforced here — guard clauses in constructors, validation in methods.
  - Rich domain model — behavior lives on the aggregate, not in services.
- **Example**: `CombinatoricProblem` enforces that decision variables have unique names, at least two states, and that constraints reference only defined variables.

### Application Layer

- **Owns**: Commands, handlers, validators, ports (repository interfaces, messaging interfaces), DTOs, error types.
- **Rules**:
  - Orchestrates domain operations but contains no business logic itself.
  - Defines interfaces for infrastructure concerns (`ICombinatoricProblemRepository`, `IUnitOfWork`, `IMessageBus`).
  - Each service has its **own** `IMediator`, `IRequest`, `IRequestHandler`, `IPipelineBehavior` — these are NOT shared.
  - Uses FluentValidation for input validation (runs via `ValidationBehavior` in the pipeline).
  - Uses FluentResults for returning success/failure without exceptions for expected errors.
- **Allowed dependencies**: Domain project, `Contracts` project, `FluentValidation`, `FluentResults`.

### Infrastructure Layer

- **Owns**: EF Core DbContext, persistence models, repository implementations, MassTransit configuration, external process wrappers.
- **Rules**:
  - Implements all interfaces defined in Application.
  - Maps domain aggregates to/from `{Noun}Model` persistence classes — the domain is never coupled to EF.
  - Registers all services via `InfrastructureModule.AddInfrastructure()` extension method.
  - Owns MassTransit consumer registration and RabbitMQ connection setup.
- **Allowed dependencies**: Application project, EF Core, MassTransit, Npgsql, etc.

### Api Layer

- **Owns**: `Program.cs`, Minimal API endpoints, request/response models, middleware pipeline configuration.
- **Rules**:
  - Thin layer — maps HTTP requests to commands, sends them through `IMediator`, maps results to HTTP responses.
  - No business logic. No direct repository access.
  - Endpoints implement `IEndpoint` interface and are registered via assembly scanning.
  - Versioned routing: `/v1/...`, `/v2/...`.
- **Allowed dependencies**: Infrastructure project, `ServiceDefaults`.

## 3. Data Flow — Submit Combinatoric Problem (Example)

```
1. HTTP POST /combinatoric-problems
   └─► SubmitCombinatoricProblemEndpoint.Execute()
       └─► IMediator.Send(SubmitCombinatoricProblemCommand)
           └─► ContextBehavior (sets MessageContext on AsyncLocal)
               └─► ValidationBehavior (FluentValidation)
                   └─► SubmitCombinatoricProblemHandler.Handle()
                       ├─ Build CombinatoricProblem (domain aggregate)
                       ├─ Add decision variables + constraints
                       ├─ Tseitin transform → SAT (DIMACS)
                       ├─ Repository.Add(problem) → maps to CombinatoricProblemModel
                       ├─ MessageBus.Publish<ICombinatoricProblemSubmitted>(...)
                       ├─ MessageBus.Publish<ISatProblemSubmitted>(...)
                       └─ UnitOfWork.SaveChanges() ← commits DB + outbox in one transaction
                           └─► MassTransit outbox delivers messages to RabbitMQ

2. RabbitMQ → SatSolver.Worker (consumer)
   └─► SatProblemSubmittedHandler (MassTransit consumer)
       └─► IMediator.Send(SolveSatProblemCommand)
           └─► ContextBehavior (sets MessageContext on AsyncLocal)
               └─► ValidationBehavior (FluentValidation)
                   └─► SolveSatProblemHandler.Handle()
                       ├─ SatProblem.Create(id, dimacs) — parse DIMACS
                       ├─ Repository.Add(problem)
                       ├─ UnitOfWork.SaveChanges()
                       ├─ ISatSolver.Solve(problem) → CryptoMiniSat process
                       ├─ problem.SetSolution(solution)
                       ├─ Repository.Update(problem)
                       ├─ MessageBus.Publish<ISatProblemSolved>(...)
                       └─ UnitOfWork.SaveChanges()
```

## 4. Messaging Architecture

### Integration Events

- Defined as **interfaces** in `Raijin.Application.Contracts`.
- Extend `IMessage` (carries `MessageId`, `CorrelationId`, `CausationId`, `Timestamp`).
- Named `I{WhatHappened}` — e.g., `ISatProblemSubmitted`, `ISatProblemSolved`.
- Published as anonymous objects: `messageBus.Publish<ISatProblemSubmitted>(new { ... })`.

### Message Context

Every request carries a `MessageContext(CorrelationId, CausationId)`:
- **HTTP-originated**: Created by `IMessageIdGenerator.NextMessageContext()` — both IDs are a new GUID.
- **Message-originated**: Constructed from the incoming `IMessage` — `CorrelationId` propagates, `CausationId` = incoming `MessageId`.
- Stored on `AsyncLocal<MessageContext>` via `ContextBehavior` pipeline.

### Outbox Pattern

All services use MassTransit's EF Core outbox. Messages are written to the outbox table in the same DB transaction as the business data. This guarantees at-least-once delivery semantics.

## 5. Key Design Decisions

| Decision | Rationale |
|---|---|
| **Custom mediator instead of MediatR** | Full control over pipeline behavior resolution, no external dependency for core orchestration |
| **Per-service messaging abstractions** | Services must be independently deployable; shared libraries create coupling |
| **Persistence models separate from domain** | Domain stays pure; EF mapping concerns don't leak into business logic |
| **FluentResults instead of exceptions** | Expected failures (validation, not-found) flow as `Result.Fail`; exceptions reserved for truly unexpected errors |
| **Tseitin transform in CombinatoricsService** | Reduction to CNF/SAT is a domain concern of the combinatorics bounded context |
| **CryptoMiniSat as external process** | Industry-grade SAT solver; called via `Process.Start` with DIMACS file I/O |
| **MigrationWorker as separate project** | Migrations run once before the service starts; Aspire `WaitForCompletion` ensures ordering |
| **One PostgreSQL server, separate databases** | Cost-effective for dev; each service has its own connection string for production isolation |

## 6. Anti-Patterns to Avoid

| Anti-Pattern | Why it's Wrong Here |
|---|---|
| **Anemic domain model** | Domain entities must enforce their own invariants — don't put validation/logic in handlers |
| **Sharing Domain/Application projects** | Creates a distributed monolith; each service must be independently evolvable |
| **Saving domain entities via EF directly** | Couples domain to persistence framework; always map to `{Noun}Model` |
| **Catching generic `Exception` in handlers** | Let unexpected exceptions bubble up to the exception handler middleware |
| **Using `.Result` or `.Wait()` on async** | Causes deadlocks; always use `await` |
| **Putting business logic in endpoints** | Endpoints are adapters; all logic goes through the command → handler pipeline |
| **Creating a shared CQRS/messaging library** | Each service must own its infrastructure abstractions to remain independently deployable |
| **Using controllers** | Project uses Minimal APIs exclusively |
| **Referencing ProblemSolvingService** | It is legacy; follow CombinatoricsService/SatSolver patterns instead |

