# Clean Architecture Skill

## Description

This skill enforces Clean Architecture principles with strict dependency rules and proper layer separation for the Raijin project.

## When to Use

Activate this skill when:
- Adding new features across layers
- Reviewing code for architecture violations
- Refactoring existing code
- User mentions "Clean Architecture", "layers", "dependencies"
- Unclear which layer code belongs in

## Clean Architecture Layers

```
┌─────────────────────────────────────┐
│         API Layer                   │  ← HTTP, thin delegation
├─────────────────────────────────────┤
│      Application Layer              │  ← Use cases, CQRS, validation
├─────────────────────────────────────┤
│    Infrastructure Layer             │  ← Database, external services
├─────────────────────────────────────┤
│        Domain Layer                 │  ← Business rules, entities
└─────────────────────────────────────┘

Dependency Rule: API → Application → Infrastructure → Domain
Critical: Domain has ZERO dependencies
```

## Layer Responsibilities

### Domain Layer

**What belongs here**:
- Business entities (aggregates, entities, value objects)
- Business rules and invariants
- Domain exceptions
- Enums representing business concepts

**What does NOT belong**:
- Database concerns (EF Core, DbContext)
- HTTP/API concerns  
- External services
- Logging, caching, I/O
- Any NuGet packages (except System.*)

**Validation**:
- ✅ `public sealed class Problem`
- ✅ `public static Problem Create(...)`  
- ✅ `if (name.Length > 100) throw new ArgumentException(...)`
- ❌ `using Microsoft.EntityFrameworkCore;`
- ❌ `using Microsoft.AspNetCore;`
- ❌ `ILogger<Problem>`

### Application Layer

**What belongs here**:
- Commands and Queries (CQRS)
- Handlers implementing `IRequestHandler<TRequest, TResponse>`
- Validators extending `AbstractValidator<TRequest>`
- DTOs for data transfer
- Interfaces for Infrastructure dependencies

**What does NOT belong**:
- HTTP request/response (that's API layer)
- Database implementation (that's Infrastructure)
- Business rules (that's Domain)

**Dependencies allowed**:
- Domain layer ✅
- FluentValidation ✅
- FluentResults ✅
- Interface definitions for Infrastructure ✅

**Validation**:
- ✅ `public sealed record CreateProblemCommand(...) : IRequest<CreateProblemResult>;`
- ✅ `public interface IProblemRepository`
- ✅ `public sealed class CreateProblemHandler(...) : IRequestHandler<...>`
- ❌ `using Microsoft.AspNetCore;` (no HTTP)
- ❌ `using Microsoft.EntityFrameworkCore;` (no EF Core)
- ❌ `public class ProblemRepository : IProblemRepository` (implementation is Infrastructure)

### Infrastructure Layer

**What belongs here**:
- DbContext and EF Core configurations
- Repository implementations
- External API clients
- File system operations
- Message bus implementations

**What does NOT belong**:
- Business logic (Domain)
- Use case orchestration (Application)
- HTTP endpoints (API)

**Dependencies allowed**:
- Application interfaces ✅
- Domain entities ✅
- Entity Framework Core ✅
- External service libraries ✅

**Validation**:
- ✅ `public sealed class ProblemRepository : IProblemRepository`
- ✅ `public sealed class MyDbContext : DbContext`
- ✅ `using Microsoft.EntityFrameworkCore;`
- ❌ Business logic in repository (delegate to Domain)
- ❌ HTTP concerns

### API Layer

**What belongs here**:
- Endpoint implementations (`IEndpoint`)
- HTTP request/response DTOs
- Result → HTTP status code mapping
- OpenAPI metadata

**What does NOT belong**:
- Business logic (use Mediator)
- Validation (Application layer handles it)
- Direct database calls (use Application layer)

**Dependencies allowed**:
- Application layer ✅
- ASP.NET Core ✅
- `IMediator` ✅

**Validation**:
- ✅ `public sealed class CreateProblemEndpoint : IEndpoint`
- ✅ `endpoint.MapPost("/v1/problems", Execute)`
- ✅ `await mediator.Send(command, ct);`
- ❌ Business logic in endpoint
- ❌ Direct repository calls
- ❌ Direct DbContext usage

## Dependency Violations

### Common Violations

❌ **Domain → Infrastructure**
```csharp
// WRONG: Domain entity referencing DbContext
public class Problem
{
    public void Save(MyDbContext db) { } // ❌ VIOLATION
}
```

✅ **Correct**:
```csharp
// Domain has business logic only
public class Problem
{
    public void UpdateName(string name) { } // ✅ OK
}

// Infrastructure handles persistence
public class ProblemRepository
{
    public async Task Save(Problem problem) { } // ✅ OK
}
```

❌ **Domain → Application**
```csharp
// WRONG: Domain using Application interfaces
public class Problem
{
    public async Task Save(IProblemRepository repo) { } // ❌ VIOLATION
}
```

❌ **Application → API**
```csharp
// WRONG: Application handler using HTTP types
public class CreateProblemHandler
{
    public async Task<IActionResult> Handle(...) { } // ❌ VIOLATION
}
```

✅ **Correct**:
```csharp
// Application returns Result<T>
public class CreateProblemHandler
{
    public async Task<Result<CreateProblemResult>> Handle(...) { } // ✅ OK
}

// API transforms to HTTP
public static async Task<Results<Created, ValidationProblem>> Execute(...)
{
    var result = await mediator.Send(command);
    return result.IsSuccess ? TypedResults.Created(...) : ...; // ✅ OK
}
```

## Decision Framework

When adding code, ask:

1. **Is this business logic?** → Domain
2. **Is this a use case or workflow?** → Application
3. **Is this database/external service?** → Infrastructure
4. **Is this HTTP interface?** → API

When in doubt:
- **Pure functions / calculations** → Domain
- **Orchestration / coordination** → Application
- **I/O / side effects** → Infrastructure
- **Request/response transformation** → API

## Verification Checklist

Before completing a feature, verify:

- [ ] Domain has no external dependencies
- [ ] Application only references Domain (and defines interfaces)
- [ ] Infrastructure implements Application interfaces
- [ ] API delegates to Application via Mediator
- [ ] No circular dependencies
- [ ] Dependency injection configured correctly

## Anti-Patterns to Avoid

❌ **Anemic Domain Model**: Entities with only getters/setters, no behavior
```csharp
// WRONG
public class Problem
{
    public Guid Id { get; set; }
    public string Name { get; set; } // ❌ Public setter
}
```

✅ **Rich Domain Model**: Entities with behavior
```csharp
// CORRECT
public class Problem
{
    public Guid Id { get; }
    public string Name { get; private set; } // ✅ Private setter
    
    public void UpdateName(string name) // ✅ Behavior method
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name;
    }
}
```

❌ **Fat Services**: Putting business logic in Application or Infrastructure
```csharp
// WRONG: Business rule in Application
public class CreateProblemHandler
{
    public async Task<Result> Handle(...)
    {
        if (name.Length > 100) // ❌ Business rule should be in Domain
            return Result.Fail("Too long");
    }
}
```

✅ **Correct**: Business rules in Domain
```csharp
// Domain enforces rules
public class Problem
{
    public static Problem Create(string name, ...)
    {
        if (name.Length > 100) // ✅ In Domain
            throw new ArgumentException("Too long");
        return new Problem(...);
    }
}

// Application uses Domain
public class CreateProblemHandler
{
    public async Task<Result> Handle(...)
    {
        var problem = Problem.Create(request.Name, ...); // ✅ Domain validates
        await _repository.Add(problem);
    }
}
```

## Quick Reference

| Layer | Purpose | Dependencies | Examples |
|-------|---------|--------------|----------|
| **Domain** | Business rules | None | Entities, Value Objects, Enums |
| **Application** | Use cases | Domain | Commands, Handlers, Validators, Interfaces |
| **Infrastructure** | External I/O | Application, Domain | Repositories, DbContext, API clients |
| **API** | HTTP interface | Application | Endpoints, Request/Response DTOs |

## See Also

- Backend Development: `.github/raijin-backend-instructions.md`
- TDD Workflow: `.claude/skills/tdd-workflow.md`
