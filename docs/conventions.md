<!-- Why this file exists: Quick-reference card for all naming, folder, and structural conventions.
     Consult this when naming a new file, class, folder, branch, or commit. -->

# Raijin — Naming & Structural Conventions

## 1. C# Naming Conventions

### General Identifiers

| Entity | Rule | Example |
|---|---|---|
| Namespace | Matches folder path: `Raijin.{Service}.{Layer}.{Subfolder}` | `Raijin.SatSolver.Application.Features.SolveSatProblem` |
| Public class | PascalCase, `sealed` by default | `SolveSatProblemHandler` |
| Abstract class | PascalCase, not sealed | `ValueObject` |
| Interface | `I` + PascalCase | `ISatProblemRepository` |
| Record (data carrier) | `sealed record`, positional syntax | `sealed record SubmitSatProblemCommand(string Dimacs, MessageContext Context)` |
| Enum | PascalCase, singular | `Satisfiability` |
| Enum member | PascalCase | `Satisfiable`, `Unsatisfiable`, `Unknown` |
| Public property | PascalCase | `public Guid Id { get; }` |
| Private field | `_camelCase` | `private readonly List<Constraint> _constrains = [];` |
| Local variable | `camelCase` | `var satProblem = SatProblem.Create(id, dimacs);` |
| Parameter | `camelCase` | `CancellationToken cancellationToken` |
| Constant | PascalCase | `private const string DimacsHeaderPrefix = "p cnf";` |
| Static readonly | PascalCase | `private static readonly ActivitySource ActivitySource = new(...)` |
| Generic type param | `T` + PascalCase | `TRequest`, `TResponse` |

### Layer-Specific Naming

#### Api Layer

| Type | Pattern | Example |
|---|---|---|
| Endpoint class | `{EndpointName}Endpoint` | `SubmitCombinatoricProblemEndpoint` |
| Request DTO | `{EndpointName}Request` | `SubmitCombinatoricProblemRequest` |
| Response DTO | `{EndpointName}Response` | `SubmitCombinatoricProblemResponse` |
| Sub-model (nested data) | `{Noun}Model` | `DecisionVariableModel` |
| Extension class | `{Purpose}Extension` or `{Type}Extensions` | `ResultExtensions`, `AddEndpointsExtension` |
| Endpoint interface | `IEndpoint` | — |

#### Application Layer

| Type | Pattern | Example |
|---|---|---|
| Command | `{Feature}Command` | `SubmitSatProblemCommand` |
| Command handler | `{Feature}Handler` | `SubmitSatProblemHandler` |
| Command result | `{Feature}Result` | `SubmitSatProblemResult` |
| Command validator | `{Feature}Validator` | `SubmitSatProblemValidator` |
| DTO | `{Noun}Dto` | `DecisionVariableDto` |
| DTO validator | `{Noun}Validator` | `DecisionVariableValidator` |
| Repository interface | `I{AggregateRoot}Repository` | `ICombinatoricProblemRepository` |
| Unit of work interface | `IUnitOfWork` | — |
| Error type | `{Noun}Error` | `ValidationError` |
| Messaging interface | `IMediator`, `IMessageBus`, `IRequest<T>`, `IRequestHandler<T, R>` | — |
| Pipeline behavior | `{Concern}Behavior` | `ValidationBehavior`, `ContextBehavior` |
| Integration event handler | `{WhatHappened}Handler` | `SatProblemSubmittedHandler` |

#### Domain Layer

| Type | Pattern | Example |
|---|---|---|
| Aggregate root | `{Noun}` (no suffix) | `CombinatoricProblem`, `SatProblem` |
| Entity | `{Noun}` | `DecisionVariable`, `Constraint` |
| Value object | `sealed record` | `Literal`, `Clause` |
| Domain exception | `{Noun}Exception` | `ParsingException` |
| Enum | `{Noun}` (singular) | `Satisfiability`, `TokenType` |

#### Infrastructure Layer

| Type | Pattern                                                                                        | Example                                        |
|---|------------------------------------------------------------------------------------------------|------------------------------------------------|
| Persistence model | `{Noun}Model` (internal)                                                                       | `CombinatoricProblemModel`                     |
| EF configuration | `{Noun}ModelConfiguration`                                                                     | `CombinatoricProblemModelConfiguration`        |
| Repository impl | `{AggregateRoot}Repository`                                                                    | `CombinatoricProblemRepository`                |
| DbContext | `{ServiceName}DbContext`                                                                       | `CombinatoricsServiceDbContext`                |
| Unit of work impl | `{ServiceName}UnitOfWork`                                                                      | `CombinatoricsServiceUnitOfWork`               |
| Migration factory | `Migration{ServiceName}DbContextFactory`                                                       | `MigrationCombinatoricsServiceDbContextFactory` |
| DI module (API only) | `AddInfrastructure()`                                                                          | `services.AddInfrastructure()`                 |
| DI module (API+Worker) | `AddInfrastructureApi()` / `AddInfrastructureWorker()`                                         | `services.AddInfrastructureApi()`              |
| MassTransit consumer | Reuses Application's `IMessageHandler<T>` impl, wrapped by MassTransit; {WhatHappened}Consumer | SatProblemSubbittedConsumer                    |

#### Contracts (Shared)

| Type | Pattern | Example |
|---|---|---|
| Base message interface | `IMessage` | Has `MessageId`, `CorrelationId`, `CausationId`, `Timestamp` |
| Integration event | `I{WhatHappened}` — **no** `Event`/`Message`/`IntegrationEvent` suffix | `ISatProblemSubmitted`, `ISatProblemSolved`, `ICombinatoricProblemSubmitted` |
| Sub-interface (data) | `I{Noun}` | `IDecisionVariable` |

## 2. File Naming Conventions

### C#

- One top-level type per file.
- File name = type name: `SubmitSatProblemCommand.cs`.
- Partial classes: same file name for main definition; `{ClassName}.{Concern}.cs` for partials if needed.

### Angular / TypeScript

| Type | Pattern | Example |
|---|---|---|
| Component | `{name}.component.ts` | `solver-form.component.ts` |
| Service | `{name}.service.ts` | `sat-problem.service.ts` |
| Model/Interface | `{name}.model.ts` | `sat-problem.model.ts` |
| Route config | `{name}.routes.ts` | `app.routes.ts` |
| Spec/test | `{name}.component.spec.ts` | `solver-form.component.spec.ts` |

## 3. Folder Naming Conventions

### C# Projects

- **Project name**: `Raijin.{ServiceName}.{Layer}` — e.g., `Raijin.SatSolver.Api`, `Raijin.CombinatoricsService.Domain`
- **Test project name**: `Raijin.{ServiceName}.{Layer}.Tests` — e.g., `Raijin.SatSolver.Domain.Tests`
- Folders inside projects use **PascalCase**: `Features/`, `Persistence/`, `Messaging/`, `Behaviors/`
- Feature folders: `Features/{FeatureName}/` — all related files colocated.
- Endpoint folders: `Endpoints/V{N}/{Aggregate}/{FeatureName}/`

### Angular

- Folders use **kebab-case**: `src/app/features/solver-form/`
- Feature folders colocate component, service, model, and spec files.

## 4. Database Naming

| Entity | Convention | Example |
|---|---|---|
| Table name | Determined by EF conventions (plural of model class) | `CombinatoricProblems` |
| Column name | PascalCase (EF default) | `Id`, `Name`, `Constraints` |
| Connection string key | kebab-case, matches Aspire resource name | `sat-solver-db`, `combinatorics-service-db` |

## 5. Aspire Resource Naming

| Resource | Convention | Example |
|---|---|---|
| Database | `{service-name}-db` | `sat-solver-db`, `identity-service-db` |
| Service API | `{service-name}-api` | `sat-solver-api`, `combinatorics-service-api` |
| Worker | `{service-name}-worker` | `sat-solver-worker` |
| Migration worker | `{service-name}-migration-worker` | `sat-solver-migration-worker` |
| Message broker | `rabbit-mq` | — |
| Cache | `{service-name}-redis-db` | `query-service-redis-db` |

## 6. Branch & Commit Conventions

### Branches

main — protected, deployable state; PRs merged here must pass all checks and follow commit message conventions.

dev — integration branch for ongoing development; PRs merged here can be more flexible but should still follow conventions.

{tiket-name} — feature branches named after the ticket or short description;

### Commit Messages

Follow **Conventional Commits**:

```
<type>(<scope>): <description>

[optional body]
```

| Type | Usage |
|---|---|
| `feat` | New feature |
| `fix` | Bug fix |
| `refactor` | Code restructuring without behavior change |
| `test` | Adding or updating tests |
| `docs` | Documentation only |
| `chore` | Build, CI, dependency updates |
| `perf` | Performance improvement |

**Scope** = service name or `shared`: `feat(sat-solver): add timeout support for CryptoMiniSat`

