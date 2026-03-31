<!-- Why this file exists: Defines the testing standards for the Raijin solution.
     Ensures consistent test structure, naming, tooling, and coverage expectations. -->

# Raijin — Testing Standards

## 1. Testing Stack

| Tool | Purpose | Version |
|---|---|---|
| xUnit v3 | Test framework | 3.2.x (`xunit.v3` / `xunit.v3.mtp-v2`) |
| Microsoft Testing Platform v3 | Test runner | via `Microsoft.NET.Test.Sdk` 18.x + `TestingPlatformDotnetTestSupport` |
| FluentAssertions | Assertion library | 8.x |
| NSubstitute | Mocking library | 5.x |
| coverlet | Code coverage | 6.x |

### Test Project Template (.csproj)

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <IsPackable>false</IsPackable>
    <RootNamespace>Raijin.{ServiceName}.{Layer}.Tests</RootNamespace>
    <TestingPlatformDotnetTestSupport>true</TestingPlatformDotnetTestSupport>
  </PropertyGroup>

  <ItemGroup>
    <Using Include="Xunit" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="FluentAssertions" Version="8.8.0" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="18.3.0" />
    <PackageReference Include="NSubstitute" Version="5.3.0" />
    <PackageReference Include="xunit.v3.mtp-v2" Version="3.2.2" />
    <PackageReference Include="coverlet.collector" Version="6.0.4">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\..\src\{ServiceName}\{Layer}\Raijin.{ServiceName}.{Layer}.csproj" />
  </ItemGroup>
</Project>
```

## 2. Test File Location & Naming

### Project Structure

```
tests/
├── {ServiceName}/
│   ├── Domain/                              # Domain unit tests (PRIMARY focus)
│   │   ├── {Aggregate}/
│   │   │   ├── {AggregateRoot}Tests.cs
│   │   │   ├── {Entity}Tests.cs
│   │   │   └── {ValueObject}Tests.cs
│   │   └── Raijin.{ServiceName}.Domain.Tests.csproj
│   ├── Application/                         # Application layer tests
│   │   ├── Features/
│   │   │   └── {Feature}/
│   │   │       └── {Feature}HandlerTests.cs
│   │   └── Raijin.{ServiceName}.Application.Tests.csproj
│   ├── Api.Tests/                           # Endpoint tests
│   │   ├── Endpoints/V1/{Aggregate}/{Feature}/
│   │   │   └── {Feature}EndpointTests.cs
│   │   └── Raijin.{ServiceName}.Api.Tests.csproj
│   └── Infrastructure.Tests/               # Integration tests
│       ├── Persistence/
│       └── Raijin.{ServiceName}.Infrastructure.Tests.csproj
```

### File Naming

| Test target | File name | Example |
|---|---|---|
| Domain aggregate | `{AggregateRoot}Tests.cs` | `SatProblemTests.cs` |
| Domain entity | `{Entity}Tests.cs` | `DecisionVariableTests.cs` |
| Domain value object | `{ValueObject}Tests.cs` | `LiteralTests.cs`, `ClauseTests.cs` |
| Domain event | `{DomainEvent}Tests.cs` | `SatProblemCreatedTests.cs` |
| Application handler | `{Feature}HandlerTests.cs` | `SubmitSatProblemHandlerTests.cs` |
| Domain event handler | `{WhatHappened}HandlerTests.cs` | `SatProblemCreatedHandlerTests.cs` |
| API endpoint | `{Feature}EndpointTests.cs` | `SubmitSatProblemEndpointTests.cs` |
| Integration test | `{Component}Tests.cs` | `SatSolverTests.cs` |

### Namespace

Test namespace mirrors the source namespace with `.Tests` suffix:

```csharp
// Source: Raijin.SatSolver.Domain.SatProblems.SatProblem
// Test:   Raijin.SatSolver.Domain.Tests.SatProblems.SatProblemTests
```

## 3. Test Categories & Traits

Every test class **must** have two `[Trait]` attributes:

```csharp
[Trait("Category", "Unit")]          // or "Integration"
[Trait("Service", "SatSolver")]      // microservice name
public class SatProblemTests { ... }
```

| Trait | Values |
|---|---|
| `Category` | `Unit`, `Integration` |
| `Service` | `SatSolver`, `CombinatoricsService`, `IdentityService`, `QueryService` |

This enables filtering: `dotnet test --filter "Category=Unit&Service=SatSolver"`.

## 4. Test Scope

### What Must Be Tested

| Priority | Layer | What | Style |
|---|---|---|---|
| **P0** | Domain | Aggregate roots, entities, value objects — all invariants, all edge cases | Unit |
| **P0** | Domain | Domain services and pure logic | Unit |
| **P0** | Domain | Domain events — correct data, naming (no `Event`/`DomainEvent` suffix) | Unit |
| **P1** | Application | Command handlers — happy path + error paths | Unit (mock repos/bus) |
| **P1** | Application | Domain event handlers — side effects + projections | Unit (mock repos/bus) |
| **P1** | Application | Validators — valid and invalid inputs | Unit |
| **P2** | Api | Endpoint mapping — request → command → response mapping | Unit (mock mediator) |
| **P3** | Infrastructure | Repository implementations, SAT solver integration | Integration |

### What Should NOT Be Tested

- EF migrations (tested implicitly by MigrationWorker)
- MassTransit consumer wiring (tested by integration/smoke tests)
- ASP.NET middleware pipeline ordering (tested by end-to-end tests)
- Private methods directly (test via public API)

## 5. Test Naming Pattern — BDD Style

Use **Given/When/Then** in method names:

```
Given{Precondition}_When{Action}_Then{ExpectedOutcome}
```

### Examples from the codebase

```csharp
GivenDimacs_WhenCreate_ThenSatProblemIsCreated()
GivenEmptySolution_WhenSetSolution_ThenSatisfiabilityIsUnsatisfiable()
GivenInvalidName_WhenConstructingDecisionVariable_ThenThrowArgumentException()
GivenValidRequest_WhenExpressionIsSatisfiable_ThenReturnsOkWithSolution()
GivenSolutionThatDoesNotSetAllTheVariables_WhenSetSolution_ThenThrowArgumentException()
```

### Comment Style

Use `// Given`, `// When`, `// Then` inside test methods:

```csharp
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
```

For parameterised tests, use `[Theory]` with `[InlineData]`:

```csharp
[Theory]
[InlineData(null)]
[InlineData("")]
[InlineData(" ")]
public void GivenInvalidName_WhenConstructingDecisionVariable_ThenThrowArgumentException(string? invalidName)
{
    // When
    Action when = () => _ = new DecisionVariable(invalidName!, ["State1", "State2"]);

    // Then
    when.Should().Throw<ArgumentException>();
}
```

## 6. Mocking Strategy

### Library: NSubstitute

- Use `Substitute.For<T>()` for interface mocks.
- Use `.Returns(...)` for setting up return values.
- Use `.Received(n).Method(...)` to verify calls.

### What to Mock

| In test for | Mock |
|---|---|
| Command handlers | `IRepository`, `IUnitOfWork`, `IMessageBus`, `ILogger<T>` |
| Endpoint tests | `IMediator`, `IMessageIdGenerator` |
| Domain tests | **Nothing** — domain is pure, no mocks needed |
| Integration tests | External processes only (if needed) |

### Example — Handler Test

```csharp
[Trait("Category", "Unit")]
[Trait("Service", "SatSolver")]
public class SubmitSatProblemHandlerTests
{
    [Fact]
    public async Task GivenValidDimacs_WhenHandling_ThenPublishesSatProblemSubmittedAndSaves()
    {
        // Given
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var messageBus = Substitute.For<IMessageBus>();
        var contextAccessor = Substitute.For<IMessageContextAccessor>();
        var messageIdGenerator = Substitute.For<IMessageIdGenerator>();
        messageIdGenerator.NextMessageId().Returns(Guid.CreateVersion7().ToString());
        contextAccessor.CurrentContext.Returns(new MessageContext("corr-1", "cause-1"));
        var logger = Substitute.For<ILogger<SubmitSatProblemHandler>>();

        var handler = new SubmitSatProblemHandler(unitOfWork, messageBus, contextAccessor, messageIdGenerator, logger);
        var command = new SubmitSatProblemCommand("p cnf 1 1\n1 0", new MessageContext("corr-1", "cause-1"));

        // When
        Result<SubmitSatProblemResult> result = await handler.Handle(command, CancellationToken.None);

        // Then
        result.IsSuccess.Should().BeTrue();
        await messageBus.Received(1).Publish<ISatProblemSubmitted>(Arg.Any<object>(), Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).SaveChanges(Arg.Any<CancellationToken>());
    }
}
```

### Example — Endpoint Test

```csharp
[Trait("Category", "Unit")]
[Trait("Service", "SatSolver")]
public class SubmitSatProblemEndpointTests
{
    [Fact]
    public async Task GivenValidRequest_WhenMediatrSucceeds_ThenReturnsOk()
    {
        // Given
        var mediator = Substitute.For<IMediator>();
        var messageIdGenerator = Substitute.For<IMessageIdGenerator>();
        messageIdGenerator.NextMessageContext().Returns(new MessageContext("id-1", "id-1"));
        var expectedResult = new SubmitSatProblemResult(Guid.CreateVersion7());
        mediator.Send(Arg.Any<SubmitSatProblemCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Ok(expectedResult));

        var request = new SubmitSatProblemRequest { Dimacs = "p cnf 1 1\n1 0" };

        // When
        var response = await SubmitSatProblemEndpoint.Execute(request, mediator, messageIdGenerator, CancellationToken.None);

        // Then
        response.Result.Should().BeOfType<Ok<SubmitSatProblemResponse>>();
    }
}
```

## 7. Test Configuration

Include `xunit.runner.json` in each test project root:

```json
{
  "$schema": "https://xunit.net/schema/current/xunit.runner.schema.json"
}
```

Reference it in `.csproj`:

```xml
<ItemGroup>
  <Content Include="xunit.runner.json" CopyToOutputDirectory="PreserveNewest" />
</ItemGroup>
```

## 8. Running Tests

```bash
# All tests
dotnet test

# Unit tests only
dotnet test --filter "Category=Unit"

# Single service
dotnet test --filter "Service=SatSolver"

# Specific layer
dotnet test tests/SatSolver/Domain/

# With coverage
dotnet test --collect:"XPlat Code Coverage"
```

