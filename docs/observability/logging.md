# Logging

Raijin logs optimize for reconstructing one request or background job end-to-end.

## Levels

| Level | Use |
| --- | --- |
| `Trace` | Extremely fine-grained internals; rare, step-by-step mechanics only. |
| `Debug` | Diagnostic detail useful while investigating, not needed in normal production reading. |
| `Information` | Expected lifecycle events proving system progress. |
| `Warning` | Abnormal but recoverable condition; request or job still handled, or fallback used. |
| `Error` | Operation failed and outcome is degraded or failed, but process can continue. |
| `Critical` | Service cannot safely continue or runtime integrity is compromised. |
| `None` | Disable a category. |

## Rules

- Use structured templates, not string interpolation.
- Prefer stable property names: `TraceId`, `SpanId`, `RequestId`, `ProblemId`, `Solver`, `JobKey`, `ElapsedMs`, `Outcome`, `ErrorCount`.
- Log one event once at the best boundary. Avoid duplicate retellings.
- Log metadata only. Never log request bodies, response bodies, SAT payloads, raw DIMACS, secrets, or user-provided content beyond safe identifiers and counts.
- Pass exception objects only when an exception exists.
- Use scopes for request, job, and problem context so nested logs inherit identifiers.

## Defaults

- Development: application categories may log `Debug`; framework categories remain constrained.
- Default/production: application logs stay mostly `Information+`; noisy framework categories stay at `Warning`.
- HTTP logs are summary-only: method, path, status code, duration, and correlation context.

## Examples

```csharp
logger.LogInformation(
    "Problem solve requested. ProblemId={ProblemId} Solver={Solver}",
    problem.Id,
    request.Solver);
```

```csharp
using IDisposable? scope = logger.BeginScope(new Dictionary<string, object?>
{
    ["ProblemId"] = problem.Id,
    ["Solver"] = satSolver.Name
});
```
