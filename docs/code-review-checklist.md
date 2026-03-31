<!-- Why this file exists: Pre-submission checklist for code reviews and Copilot-generated code.
     Every item here is a concrete, verifiable check — not a vague suggestion. -->

# Raijin — Code Review Checklist

Use this before submitting a PR or accepting Copilot-generated code.

---

## ✅ Architecture

- [ ] Domain layer has **zero** external package references (only `System.*`)
- [ ] No business logic in Api or Infrastructure layers
- [ ] Commands flow through `IMediator` → pipeline → handler (not called directly)
- [ ] Handlers return `Result<T>` or `Result` (FluentResults), not raw exceptions for expected failures
- [ ] Integration events are published via `IMessageBus.Publish<IContract>(...)`, not directly via MassTransit
- [ ] `UnitOfWork.SaveChanges()` is called **before** publishing integration events (persist-then-publish)
- [ ] No cross-service references to Application or Domain projects
- [ ] Each service owns its own `IMediator`, `IRequest`, `IRequestHandler`, `IPipelineBehavior`
- [ ] MigrationWorker is a separate project using `BackgroundService` pattern
- [ ] Domain events are `sealed record` types colocated with their aggregate in the Domain layer
- [ ] Domain event names follow `{WhatHappened}` (past tense, no `Event`/`DomainEvent` suffix)

## ✅ Naming

- [ ] Endpoint: `{Name}Endpoint`, request: `{Name}Request`, response: `{Name}Response`
- [ ] Command: `{Name}Command`, handler: `{Name}Handler`, result: `{Name}Result`
- [ ] Validator: `{Name}Validator` (for commands), `{Noun}Validator` (for DTOs)
- [ ] Application DTOs: `{Noun}Dto`; API sub-models: `{Noun}Model`
- [ ] Persistence models: `{Noun}Model` (internal class)
- [ ] Repository: `I{AggregateRoot}Repository` (interface), `{AggregateRoot}Repository` (impl)
- [ ] DbContext: `{ServiceName}DbContext`; UnitOfWork: `{ServiceName}UnitOfWork`
- [ ] Integration event: `I{WhatHappened}` — no `Event`/`Message`/`IntegrationEvent` suffix
- [ ] Domain event: `{WhatHappened}` — no `Event`/`DomainEvent` suffix
- [ ] Integration event handler: `{WhatHappened}Handler` — no `EventHandler`/`MessageHandler`/`Consumer` suffix
- [ ] Domain event handler: `{WhatHappened}Handler` — no `EventHandler`/`DomainEventHandler` suffix
- [ ] Command/query handler: `{Feature}Handler` — no `CommandHandler`/`RequestHandler`/`QueryHandler` suffix
- [ ] Validator: no `CommandValidator`/`RequestValidator` compound suffix
- [ ] Pipeline behavior: `{Concern}Behavior` — no `PipelineBehavior` suffix
- [ ] Result: `{Feature}Result` — no `CommandResult`/`HandlerResult` suffix
- [ ] Infrastructure module: `AddInfrastructure()` (API only) or `AddInfrastructureApi()`/`AddInfrastructureWorker()` (both)
- [ ] MassTransit consumer: uses generic `MessageConsumer<TMessage>`, not individual consumer classes
- [ ] Namespaces match folder structure exactly
- [ ] File name matches type name

## ✅ Code Style

- [ ] File-scoped namespace (not block-scoped)
- [ ] `sealed` on all classes unless inheritance is explicitly needed
- [ ] Primary constructors for DI injection
- [ ] `var` when type is obvious; explicit type for interface-typed variables
- [ ] Expression-bodied members for single-expression methods
- [ ] `sealed record` for commands, results, DTOs, value objects
- [ ] Collection initializer `[]` syntax where applicable
- [ ] `IReadOnlyList<T>` for public collection properties
- [ ] Guard clauses: `ArgumentNullException.ThrowIfNull()`, `ArgumentException.ThrowIfNullOrWhiteSpace()`
- [ ] `CancellationToken` accepted and passed in all async methods
- [ ] No `.Result`, `.Wait()`, or `.GetAwaiter().GetResult()` on tasks
- [ ] No `async void`
- [ ] Explicit `[FromBody]`, `[FromServices]`, `[FromQuery]` on endpoint parameters
- [ ] `Nullable` and `ImplicitUsings` enabled in all `.csproj` files

## ✅ Persistence

- [ ] Domain entities are **never** passed to EF directly
- [ ] Persistence models are `internal class` with `{Noun}Model` naming
- [ ] Repository maps domain ↔ persistence model
- [ ] EF configurations use `IEntityTypeConfiguration<TModel>` in separate files
- [ ] Configurations registered via `modelBuilder.ApplyConfigurationsFromAssembly()`
- [ ] Connection strings retrieved via `IConfiguration.GetConnectionString()` (Aspire service discovery)

## ✅ Messaging

- [ ] Integration event interfaces live in `Contracts` project
- [ ] All events extend `IMessage` (includes `MessageId`, `CorrelationId`, `CausationId`, `Timestamp`)
- [ ] Events published as anonymous objects matching the interface shape
- [ ] `MessageContext` propagated through `ContextBehavior` pipeline
- [ ] MassTransit uses kebab-case endpoint name formatter

## ✅ Testing

- [ ] Test class has `[Trait("Category", "Unit")]` or `[Trait("Category", "Integration")]`
- [ ] Test class has `[Trait("Service", "{ServiceName}")]`
- [ ] Test methods follow `Given{State}_When{Action}_Then{Expected}` naming
- [ ] Test body uses `// Given`, `// When`, `// Then` comments
- [ ] Uses FluentAssertions for all assertions (`.Should()`)
- [ ] Uses NSubstitute for mocking (`Substitute.For<T>()`)
- [ ] Domain tests have **no mocks** — domain is pure
- [ ] Handler tests mock repositories, unit of work, message bus, logger
- [ ] Endpoint tests mock mediator and message ID generator
- [ ] `Theory` + `InlineData` for parameterised tests

## ✅ API Endpoints

- [ ] Implements `IEndpoint` interface with `Map(IEndpointRouteBuilder)` method
- [ ] Route versioned: `/v1/...`
- [ ] Uses `.WithName()` and `.WithTags()` for OpenAPI
- [ ] Returns `Results<Ok<T>, ValidationProblem, InternalServerError>` typed results
- [ ] Handles `ValidationError` → `TypedResults.ValidationProblem()`
- [ ] Registered via assembly scanning (`AddEndpoints()` / `MapEndpoints()`)

## ✅ Validation

- [ ] FluentValidation validator exists for every command
- [ ] Validator registered via `AddValidatorsFromAssembly()` in `ApplicationModule`
- [ ] `ValidationBehavior` pipeline runs validation before handler
- [ ] Validation failures returned as `Result.Fail(validationErrors)`, not thrown

## ✅ Angular (SPA)

- [ ] Standalone components (no NgModules)
- [ ] Component selector prefix: `rjn-`
- [ ] OnPush change detection preferred
- [ ] Strict TypeScript (`strict: true`)
- [ ] Single quotes, 2-space indent
- [ ] File naming: kebab-case

## ✅ Performance

- [ ] No N+1 query patterns in repositories
- [ ] `DbContextPool` used (not `AddDbContext`)
- [ ] No unbounded `ToList()` on large datasets — use pagination
- [ ] `CancellationToken` propagated to DB queries and HTTP calls
- [ ] No synchronous I/O blocking
- [ ] Response compression and caching configured in ApiGateway

## ✅ Security

- [ ] Authentication middleware before authorization in pipeline
- [ ] HTTPS redirection enabled
- [ ] CORS configured in ApiGateway only
- [ ] Rate limiting configured in ApiGateway
- [ ] No secrets in source code — use Aspire user secrets or environment variables
- [ ] `HSTS` enabled in production

