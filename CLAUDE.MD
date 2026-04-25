# Claude Agent Configuration for Raijin

.NET 10 microservices platform for solving combinatorial optimization problems.

## Overview

Raijin uses Claude as AI pair-programmer for:
- **.NET 10 microservices** with Clean Architecture
- **React 19 + TypeScript** frontend
- **.NET Aspire** orchestration

## Configuration Structure

```
.claude/
├── CLAUDE.MD               ← You are here (documentation)
├── instructions.md         ← Main system prompt (core behavior)
├── agents/                 ← Specialized agents for roles
│   ├── backend-developer.md
│   └── frontend-developer.md
└── skills/                 ← Reusable capabilities
    └── clean-architecture.md
```

## File Descriptions

### instructions.md - Core System Prompt

Foundation for all Claude interactions. Defines:
- **Identity**: Expert .NET and React developer
- **Core Principles**: Clean Architecture, CQRS, Result pattern
- **Technology Stack**: .NET 10, React 19, PostgreSQL
- **Coding Standards**: Async/await, null safety, error handling
- **Forbidden Behaviors**: Architecture violations

Always active — baseline behavior for every interaction.

### agents/ - Specialized Roles

**Backend Developer** (`backend-developer.md`)
- .NET 10 backend specialist
- Triggers: Domain, Application, Infrastructure, API, repositories, handlers, EF Core
- Creates: Entities, CQRS commands/queries, repositories, endpoints
- Follows: Clean Architecture, layer-specific patterns

**Frontend Developer** (`frontend-developer.md`)
- React 19 + TypeScript specialist
- Triggers: React, components, hooks, UI, state, Vite
- Creates: Functional components, custom hooks, API clients, types
- Follows: React patterns, TypeScript best practices

### skills/ - Reusable Capabilities

**Clean Architecture** (`clean-architecture.md`)
- Enforces layer separation + dependency rules
- Decision framework for layer placement
- Violation detection and correction

## How It Works

Claude activates agents based on request context.

**Backend Feature** — "Add a CreateProblem command"
- Activation: Backend Agent + Clean Architecture
1. Create Problem entity with factory method
2. Implement handler + validator
3. Create endpoint
4. Verify build succeeds

**Frontend Component** — "Create a ProblemList component"
- Activation: Frontend Agent
1. Define TypeScript interface for Problem
2. Create functional component with props
3. Add custom `useProblem()` hook
4. Integrate with API client
5. Style with CSS module

## Key Architectural Concepts

### Clean Architecture Layers

```
API           ← HTTP endpoints, minimal logic
    ↓
Application   ← Use cases, commands, queries
    ↓  
Infrastructure← Database, external services
    ↓
Domain        ← Business rules (zero dependencies)
```

Dependencies point inward. Domain is pure business logic.

### CQRS Pattern

- **Commands**: Mutate state (Create, Update, Delete) → `Result<T>`
- **Queries**: Read data (Get, List) → `Result<T>`
- **Mediator**: Central dispatch via `IMediator.Send()`
- **Handlers**: One handler per command/query

### Result Pattern

Using **FluentResults** instead of exceptions:

```csharp
// Success
return Result.Ok(new MyResult(id));

// Failure
return Result.Fail(new ValidationError("field", "message"));

// Check
if (result.IsFailed)
    return TypedResults.BadRequest(result.ToProblemDetails());
```

### Vertical Slices

Features organized by capability, not technical layer:

```
Features/Problems/CreateProblem.cs
  ├── CreateProblemCommand (record)
  ├── CreateProblemResult (record)
  ├── CreateProblemHandler (class)
  └── CreateProblemValidator (class)
```

## Technology Stack

### Backend (.NET 10)
- .NET Aspire (orchestration)
- ASP.NET Core Minimal APIs
- Entity Framework Core + PostgreSQL
- FluentValidation
- FluentResults
- Quartz.NET (background jobs)

### Frontend
- React 19
- TypeScript 5.9
- Vite 8
- ESLint 9

## Common Workflows

| Task | Agents/Skills | Result |
|------|---------------|--------|
| Add CreateProblem command | Backend + Clean Arch | Full vertical slice |
| Create ProblemList component | Frontend | Component + Hook + Types |
| Fix architecture violation | Clean Architecture | Corrected layer placement |

## Activation Triggers

**Backend Agent**: Domain, Application, Infrastructure, API, repository, handler, entity, command, query, mediator, EF Core, migration

**Frontend Agent**: React, component, hook, TypeScript, UI, state, Vite, frontend, SPA

**Clean Architecture Skill**: Clean Architecture, layers, dependencies, violation, boundary

## Best Practices

DO:
- Mention layer names ("working on Domain layer")
- Reference patterns ("use Clean Architecture")
- Be specific about technology
- Keep `instructions.md` focused on core behavior
- Put detailed patterns in agents
- Extract reusable processes into skills

DON'T:
- Skip architectural layers
- Violate dependency rules
- Duplicate content
- Add tech details to `instructions.md`
- Create overlapping agents

## Troubleshooting

**Claude violates dependency rules** → "Follow Clean Architecture strictly"

**Claude uses outdated patterns** → Check `instructions.md` mentions .NET 10

## Extending

**Add Agent**: Create `.claude/agents/my-agent.md`, define role/triggers/workflows, include examples.

**Add Skill**: Create `.claude/skills/my-skill.md`, describe capability, add step-by-step guidance + code samples.

## Resources

- **Claude Docs**: https://docs.anthropic.com/

---

**Version**: 1.0 | **Created**: 2025 | **Maintained by**: Raijin Development Team
