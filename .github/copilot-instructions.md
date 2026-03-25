<!-- Why this file exists: This is the primary instruction file for GitHub Copilot.
     It ensures every suggestion matches the project's architecture, naming, and style. -->

# Raijin — Copilot Instructions

## Project Overview

Raijin is a distributed microservices platform for solving combinatorial optimisation problems by reducing them to SAT (Boolean satisfiability) and delegating solving to the CryptoMiniSat engine. The backend is .NET 10 / C# with .NET Aspire orchestration; the frontend is an Angular 19 SPA. Services communicate asynchronously via RabbitMQ (MassTransit) and each owns its own PostgreSQL database.

## Tech Stack

| Layer | Technology |
|---|---|
| Runtime | .NET 10, C# 13 |
| Orchestration | .NET Aspire 9 (`Aspire.AppHost.Sdk`) |
| API | ASP.NET Core Minimal APIs (no controllers) |
| Messaging | MassTransit 8 + RabbitMQ |
| Persistence | EF Core 10, Npgsql (PostgreSQL) |
| Validation | FluentValidation 12 |
| Error handling | FluentResults 4 |
| Testing | xUnit v3, FluentAssertions 8, NSubstitute 5, Microsoft Testing Platform v3 |
| Frontend | Angular 19, TypeScript 5.7, RxJS 7 |
| Containers | Docker (multi-stage builds) |
| Observability | OpenTelemetry (traces, metrics, logs via Aspire) |

## Architecture Overview

```
┌─────────────┐   HTTP   ┌────────────┐  service-discovery  ┌──────────────────────┐
│  Angular SPA ├─────────►│ ApiGateway ├────────────────────►│  Service APIs        │
└─────────────┘          └────────────┘                     │  (CombinatoricsService│
                                                            │   SatSolver, Identity,│
                                                            │   QueryService)       │
                                                            └──────────┬───────────┘
                                                                       │ MassTransit
                                                            ┌──────────▼───────────┐
                                                            │  Workers / Consumers │
                                                            └──────────────────────┘
```

Each microservice follows **Clean Architecture** with four projects:

| Project | Purpose |
|---|---|
| `Api` | HTTP entry point — Minimal API endpoints, request/response models, startup |
| `Application` | Use cases — commands, handlers, validators, ports (repository interfaces, messaging abstractions) |
| `Domain` | Core business logic — aggregates, entities, value objects, domain services. **Zero external dependencies.** |
| `Infrastructure` | Adapters — EF Core, MassTransit, external process wrappers, DI module registration |

Additional projects per service when needed:

| Project | Purpose |
|---|---|
| `MigrationWorker` | Standalone `BackgroundService` that runs EF Core migrations then exits |
| `Worker` | Long-running consumer host (e.g., the SAT solver worker) |

Cross-cutting shared projects:

| Project | Purpose |
|---|---|
| `Contracts` | Integration event interfaces shared between services |
| `ServiceDefaults` | Aspire service defaults (health checks, OpenTelemetry, resilience) |

### Key Patterns

- **CQRS** — Commands go through `IMediator` → `IPipelineBehavior` pipeline → `IRequestHandler`. Handlers return `Result<T>` (FluentResults).
- **Pipeline Behaviors** — `LoggingBehavior` (timing & result logging) → `ValidationBehavior` (FluentValidation) → Handler.
- **Repository + Unit of Work** — Repository interfaces live in `Application/Persistence`. Implementations in `Infrastructure/Persistence`. Always call `IUnitOfWork.SaveChanges()` at the end.
- **Persistence Models** — Domain aggregates are **never** saved directly. Map domain objects to `{Noun}Model` classes in `Infrastructure/Persistence/Models`.
- **Integration Events** — Defined as interfaces in `Contracts` project. Published via `IMessageBus.Publish<TContract>(anonymousObject)`.
- **Persist-then-Publish** — Always call `unitOfWork.SaveChanges()` **before** publishing integration events via `IMessageBus`. This ensures the entity is committed to the database before any consumer can attempt to read it.
- **Each service is fully independent** — owns its own messaging abstractions (`IMediator`, `IRequest`, `IPipelineBehavior`, etc.), its own persistence, its own models. No shared Application/Domain code between services. This is intentional to avoid a distributed monolith.

## Folder Structure Map

```
src/
├── AppHost/                          # .NET Aspire orchestrator
├── ApiGateway/                       # Edge gateway (CORS, rate-limiting, auth, routing)
├── Contracts/                        # Integration event interfaces (shared)
├── ServiceDefaults/                  # Aspire service defaults (shared)
├── Spa/                              # Angular 19 SPA
│   └── src/app/
├── {ServiceName}/
│   ├── Api/
│   │   ├── Endpoints/
│   │   │   ├── IEndpoint.cs
│   │   │   └── V1/{Aggregate}/{Feature}/
│   │   │       ├── {Feature}Endpoint.cs
│   │   │       ├── {Feature}Request.cs
│   │   │       ├── {Feature}Response.cs
│   │   │       └── {Noun}Model.cs          # API sub-models
│   │   ├── Extensions/
│   │   └── Program.cs
│   ├── Application/
│   │   ├── Features/{Feature}/
│   │   │   ├── {Feature}Command.cs
│   │   │   ├── {Feature}Handler.cs
│   │   │   ├── {Feature}Validator.cs
│   │   │   ├── {Feature}Result.cs
│   │   │   └── {Noun}Dto.cs               # Application-layer DTOs
│   │   ├── Errors/
│   │   ├── Messaging/
│   │   │   ├── Behaviors/
│   │   │   ├── IMediator.cs, IRequest.cs, IRequestHandler.cs, etc.
│   │   ├── Persistence/
│   │   │   ├── I{Noun}Repository.cs
│   │   │   └── IUnitOfWork.cs
│   │   └── Validation/
│   ├── Domain/
│   │   └── {Aggregate}/               # Aggregate root + child entities + value objects
│   ├── Infrastructure/
│   │   ├── InfrastructureModule.cs
│   │   ├── Messaging/                  # MassTransit, ServiceProviderMediator, etc.
│   │   └── Persistence/
│   │       ├── {ServiceName}DbContext.cs
│   │       ├── {ServiceName}UnitOfWork.cs
│   │       ├── Configurations/
│   │       ├── Models/                  # EF persistence models ({Noun}Model)
│   │       ├── Repositories/
│   │       └── Migrations/
│   ├── MigrationWorker/               # (optional)
│   └── Worker/                         # (optional)
tests/
├── {ServiceName}/
│   ├── Domain/                         # Domain unit tests (primary focus)
│   ├── Application/                    # Application layer tests
│   ├── Api/                      # Endpoint tests
│   └── Infrastructure/           # Integration tests
```

## Naming Conventions

### C# — General

| Entity | Convention | Example |
|---|---|---|
| Namespace | Matches folder path exactly | `Raijin.SatSolver.Application.Features.SubmitSatProblem` |
| Interface | `I` prefix, PascalCase | `ISatProblemRepository`, `IUnitOfWork` |
| Class | PascalCase, `sealed` by default | `SolveSatProblemHandler` |
| Record | PascalCase, prefer positional records for DTOs/commands | `record SubmitSatProblemCommand(string Dimacs, MessageContext Context)` |
| Enum | PascalCase, singular name | `Satisfiability { Satisfiable, Unsatisfiable, Unknown }` |
| Private field | `_camelCase` with underscore prefix | `_decisionVariables` |
| Local variable | `camelCase`, use `var` when type is obvious | `var satProblem = new SatProblem(id);` |
| Constant | PascalCase | `private const string DimacsHeaderPrefix = "p cnf";` |

### C# — Layer-Specific

| Layer | Type | Naming Pattern | Example |
|---|---|---|---|
| **Api** | Endpoint handler | `{EndpointName}Endpoint` | `SubmitSatProblemEndpoint` |
| **Api** | Request DTO | `{EndpointName}Request` | `SubmitSatProblemRequest` |
| **Api** | Response DTO | `{EndpointName}Response` | `SubmitSatProblemResponse` |
| **Api** | Sub-model | `{Noun}Model` | `DecisionVariableModel` |
| **Application** | Command | `{Feature}Command` | `SubmitCombinatoricProblemCommand` |
| **Application** | Handler | `{Feature}Handler` | `SubmitCombinatoricProblemHandler` |
| **Application** | Result | `{Feature}Result` | `SubmitCombinatoricProblemResult` |
| **Application** | Validator | `{Feature}Validator` | `SubmitCombinatoricProblemValidator` |
| **Application** | DTO | `{Noun}Dto` | `DecisionVariableDto` |
| **Application** | DTO Validator | `{Noun}Validator` | `DecisionVariableValidator` |
| **Application** | Integration event handler | `{WhatHappened}Handler` | `SatProblemSubmittedHandler` |
| **Application** | Domain event handler | `{WhatHappened}Handler` | `SatProblemCreatedHandler` |
| **Application** | Pipeline behavior | `{Concern}Behavior` | `ValidationBehavior` |
| **Application** | DI module | `ApplicationModule` | `ApplicationModule.AddApplication()` |
| **Domain** | Aggregate root | `{Noun}` (no suffix) | `CombinatoricProblem` |
| **Domain** | Entity | `{Noun}` (no suffix) | `DecisionVariable` |
| **Domain** | Value object | `sealed record` | `Literal`, `Clause` |
| **Domain** | Domain event | `{WhatHappened}` (no suffix) | `SatProblemCreated` |
| **Domain** | Domain exception | `{Noun}Exception` | `ParsingException` |
| **Infrastructure** | Persistence model | `{Noun}Model` | `CombinatoricProblemModel` |
| **Infrastructure** | EF Configuration | `{Noun}ModelConfiguration` | `CombinatoricProblemModelConfiguration` |
| **Infrastructure** | Repository | `{Noun}Repository` | `CombinatoricProblemRepository` |
| **Infrastructure** | DbContext | `{ServiceName}DbContext` | `SatSolverDbContext` |
| **Infrastructure** | UnitOfWork | `{ServiceName}UnitOfWork` | `SatSolverUnitOfWork` |
| **Infrastructure** | DI module | `InfrastructureModule` | `InfrastructureModule.AddInfrastructure()` |
| **Infrastructure** | MassTransit consumer | `MessageConsumer<TMessage>` (generic) | `MessageConsumer<ISatProblemSubmitted>` |
| **Contracts** | Integration event | `I{WhatHappened}` (no suffix) | `ISatProblemSubmitted` |

### Naming Suffix Rules (Banned Suffixes)

Every type uses a **short role suffix** — never a compound or redundant suffix. The table below lists banned alternatives for each type category.

| Type | Correct Suffix | Banned Suffixes (never use) |
|---|---|---|
| Integration event | *(none)* — `I{WhatHappened}` | `Event`, `Message`, `IntegrationEvent` |
| Domain event | *(none)* — `{WhatHappened}` | `Event`, `DomainEvent` |
| Command/Query handler | `Handler` | `RequestHandler`, `CommandHandler`, `QueryHandler` |
| Integration event handler | `Handler` | `EventHandler`, `MessageHandler`, `IntegrationEventHandler`, `Consumer` |
| Domain event handler | `Handler` | `EventHandler`, `DomainEventHandler` |
| Validator | `Validator` | `CommandValidator`, `RequestValidator`, `QueryValidator` |
| Result | `Result` | `CommandResult`, `HandlerResult`, `Response` (in Application layer) |
| Pipeline behavior | `Behavior` | `PipelineBehavior` |
| Command | `Command` | `Request` (in Application layer — `IRequest` is the interface, `Command` is the class suffix) |
| DTO | `Dto` | `DataTransferObject`, `Model` (in Application layer) |
| Error type | `Error` | `Exception` (reserve `Exception` for truly exceptional/unrecoverable situations) |

### Infrastructure Module Naming

- Service with **API only** → `AddInfrastructure()`
- Service with **API + Worker** → `AddInfrastructureApi()` / `AddInfrastructureWorker()`

### MassTransit Consumer Naming

- Use the generic `MessageConsumer<TMessage>` wrapper — do **not** create individual `{WhatHappened}Consumer` classes.
- Register as `x.AddConsumer<MessageConsumer<ISatProblemSubmitted>>()` in `InfrastructureModule`.

### Angular/TypeScript

| Entity | Convention | Example |
|---|---|---|
| Component selector prefix | `rjn-` | `rjn-solver-form` |
| File naming | kebab-case | `solver-form.component.ts` |
| Class | PascalCase | `SolverFormComponent` |
| Service | PascalCase + `Service` suffix | `SatProblemService` |
| Interface | PascalCase, `I` prefix (C#-like style) | `ISolverResult` |
| Constants | UPPER_SNAKE_CASE | `MAX_VARIABLES` |

## Code Style Rules

### C#

- **File-scoped namespaces** — always.
- **`sealed`** — seal all classes unless inheritance is explicitly required.
- **Explicit interface access modifiers** — always use explicit `public` on interface members (properties, methods). Never rely on the implicit default.
- **Primary constructors** — use for DI injection in handlers, services, and repositories.
- **`var`** — use when the type is obvious from the right side; use explicit type for interface-typed variables.
- **Expression-bodied members** — use for single-expression methods and properties.
- **Nullable reference types** — always enabled (`<Nullable>enable</Nullable>`).
- **Implicit usings** — always enabled.
- **Records** — use `sealed record` for commands, results, DTOs, value objects.
- **Collections** — initialize with `[]` collection expression syntax, expose as `IReadOnlyList<T>`.
- **Guard clauses** — use `ArgumentException.ThrowIfNullOrWhiteSpace()`, `ArgumentNullException.ThrowIfNull()`, `ArgumentOutOfRangeException.ThrowIfNegativeOrZero()`.
- **CancellationToken** — always accept and pass through in async methods.
- **async/await** — always use; never `.Result` or `.Wait()`.

### Angular/TypeScript

- **Strict mode** — `strict: true` in `tsconfig.json`.
- **Single quotes** for strings.
- **2-space indent**.
- **Standalone components** (no NgModules).
- **OnPush change detection** preferred.

## What Copilot Should ALWAYS Do

1. Follow Clean Architecture layer boundaries — Domain has **zero** external package references.
2. Return `Result<T>` or `Result` (FluentResults) from all command handlers.
3. Validate commands with FluentValidation via the `ValidationBehavior` pipeline.
4. Publish integration events as `IMessageBus.Publish<IContractInterface>(anonymousObject)`.
5. Persist first, then publish — call `unitOfWork.SaveChanges()` **before** publishing integration events via `IMessageBus`.
6. Map domain aggregates to `{Noun}Model` persistence classes — never pass domain entities to EF directly.
7. Use `IEndpoint` interface for all Minimal API endpoints, registered via assembly scanning.
8. Use `sealed` on classes and `sealed record` for data carriers.
9. Use primary constructors for dependency injection.
10. Use `Guid.CreateVersion7()` for new entity IDs.
11. Include `CancellationToken` in all async signatures.
12. Use `[FromBody]`, `[FromServices]`, `[FromQuery]` explicitly in endpoint parameters.
13. Keep each microservice's messaging abstractions (`IMediator`, `IRequest`, etc.) inside that service's Application project — do **not** create a shared library.
14. Place integration event interfaces in the `Contracts` project with name pattern `I{WhatHappened}` — no `Event`, `Message`, or `IntegrationEvent` suffix.
15. Name integration event handlers `{WhatHappened}Handler` — no `EventHandler`, `MessageHandler`, or `Consumer` suffix.
16. Name domain events `{WhatHappened}` (past tense, no suffix) — no `Event` or `DomainEvent` suffix.
17. Name domain event handlers `{WhatHappened}Handler` — same short suffix as integration event handlers.
18. Name command/query handlers `{Feature}Handler` — no `CommandHandler`, `RequestHandler`, or `QueryHandler` suffix.
19. Name validators `{Feature}Validator` — no `CommandValidator` or `RequestValidator` suffix.
20. Name pipeline behaviors `{Concern}Behavior` — no `PipelineBehavior` suffix.
21. Follow the **Naming Suffix Rules** table — use only the short role suffix for each type category.

## What Copilot Should NEVER Do

1. **Never** use MediatR — the project uses a custom `ServiceProviderMediator`.
2. **Never** use controller-based APIs — only Minimal APIs with `IEndpoint`.
3. **Never** save domain aggregates/entities directly to EF Core — always map to `{Noun}Model`.
4. **Never** share Application or Domain projects between microservices.
5. **Never** create a shared messaging/CQRS library — each service owns its abstractions.
6. **Never** use `async void`.
7. **Never** swallow exceptions silently.
8. **Never** use `.Result`, `.Wait()`, or `GetAwaiter().GetResult()` on tasks.
9. **Never** add package references to Domain projects (they must remain dependency-free).
10. **Never** use block-scoped namespaces.
11. **Never** use NgModules in Angular — use standalone components only.
12. **Never** put business logic in the Api or Infrastructure layer.
13. **Never** name integration events with `Event`, `Message`, or `IntegrationEvent` suffix.
14. **Never** name domain events with `Event` or `DomainEvent` suffix — use `{WhatHappened}` (past tense, no suffix).
15. **Never** use compound handler suffixes — use `Handler`, not `CommandHandler`, `RequestHandler`, `QueryHandler`, `EventHandler`, or `MessageHandler`.
16. **Never** use compound validator suffixes — use `Validator`, not `CommandValidator` or `RequestValidator`.
17. **Never** use compound behavior suffixes — use `Behavior`, not `PipelineBehavior`.
18. **Never** use compound result suffixes — use `Result`, not `CommandResult` or `HandlerResult`.

## Code Pattern Examples (from the actual codebase)

### Command (Application Layer)

```csharp
// File: Application/Features/SubmitSatProblem/SubmitSatProblemCommand.cs
using Raijin.SatSolver.Application.Messaging;

namespace Raijin.SatSolver.Application.Features.SubmitSatProblem;

public sealed record SubmitSatProblemCommand(string Dimacs, Guid? SatProblemId = null)
    : IRequest<SubmitSatProblemResult>;
```

### Handler (Application Layer)

```csharp
// File: Application/Features/SubmitCombinatoricProblem/SubmitCombinatoricProblemHandler.cs
public sealed class SubmitCombinatoricProblemHandler(
    ICombinatoricProblemRepository combinatoricProblemRepository,
    IUnitOfWork unitOfWork,
    IMessageBus messageBus,
    ILogger<SubmitCombinatoricProblemHandler> logger
) : IRequestHandler<SubmitCombinatoricProblemCommand, SubmitCombinatoricProblemResult>
{
    public async Task<Result<SubmitCombinatoricProblemResult>> Handle(
        SubmitCombinatoricProblemCommand request, CancellationToken cancellationToken)
    {
        // ... build domain object, persist first
        await unitOfWork.SaveChanges(cancellationToken);
        // ... then publish integration events
        return new SubmitCombinatoricProblemResult(combinatoricProblemId);
    }
}
```

### Endpoint (Api Layer)

```csharp
// File: Api/Endpoints/V1/Sat/SubmitSatProblem/SubmitSatProblemEndpoint.cs
public class SubmitSatProblemEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder endpoint)
    {
        endpoint.MapPost("/v1/sat", Execute)
            .WithName("SubmitSatProblem")
            .WithTags("sat");
    }

    private static async Task<Results<Ok<SubmitSatProblemResponse>, ValidationProblem, InternalServerError>> Execute(
        [FromBody] SubmitSatProblemRequest request,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        Result<SubmitSatProblemResult> result = await mediator.Send(
            new SubmitSatProblemCommand(request.Dimacs),
            cancellationToken);

        if (result.IsSuccess)
            return TypedResults.Ok(new SubmitSatProblemResponse { SatProblemId = result.Value.SatProblemId });
        if (result.HasError<ValidationError>())
            return TypedResults.ValidationProblem(errors: result.ToValidationErrorDictionary());
        return TypedResults.InternalServerError();
    }
}
```

### Integration Event (Contracts)

```csharp
// File: Contracts/ISatProblemSubmitted.cs
namespace Raijin.Application.Contracts;

public interface ISatProblemSubmitted : IMessage
{
    public string SatProblemId { get; }
    public string Dimacs { get; }
}
```

### Domain Event (Domain Layer)

```csharp
// File: Domain/SatProblems/SatProblemCreated.cs
// Named {WhatHappened} — NO 'Event' or 'DomainEvent' suffix.
namespace Raijin.SatSolver.Domain.SatProblems;

public sealed record SatProblemCreated(Guid SatProblemId, string Dimacs);
```

### Domain Entity

```csharp
// File: Domain/CombinatoricProblems/CombinatoricProblem.cs
public class CombinatoricProblem(Guid id)
{
    private readonly Dictionary<string, DecisionVariable> _decisionVariables = [];
    private readonly List<Constraint> _constrains = [];

    public Guid Id { get; } = id;
    public IReadOnlyList<DecisionVariable> DecisionVariables => _decisionVariables.Values.ToList();
    public IReadOnlyList<Constraint> Constraints => _constrains;

    public void AddDecisionVariable(string name, string[] states) { /* ... */ }
    public void AddConstrain(string formula) => AddConstrain(new Constraint(formula));
}
```

### Persistence Model (Infrastructure Layer)

```csharp
// File: Infrastructure/Persistence/Models/CombinatoricProblemModel.cs
namespace Raijin.CombinatoricsService.Infrastructure.Persistence.Models;

internal class CombinatoricProblemModel
{
    public Guid Id { get; set; }
    public List<DecisionVariableModel> DecisionVariables { get; set; }
    public string[] Constraints { get; set; }
}
```

### Unit Test (BDD Style)

```csharp
// File: tests/SatSolver/Domain/SatProblems/SatProblemTests.cs
[Trait("Category", "Unit")]
[Trait("Service", "SatSolver")]
public class SatProblemTests
{
    [Fact]
    public void GivenDimacs_WhenCreate_ThenSatProblemIsCreated()
    {
        // Given
        var dimacs = "p cnf 3 2\n1 -3 0\n-1 2 3 0";

        // When
        var satProblem = SatProblem.Create(Guid.CreateVersion7(), dimacs);

        // Then
        satProblem.Should().NotBeNull();
    }
}
```

