using Raijin.CombinatoricsService.Api.Extensions;
using Raijin.CombinatoricsService.Application;
using Raijin.CombinatoricsService.Infrastructure;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddProblemDetails();
builder.Services.AddHttpLogging(options => options.CombineLogs = true);
builder.Services.AddOpenApi();
builder.Services.AddEndpoints();
builder.AddInfrastructure();
builder.Services.AddApplication();

WebApplication app = builder.Build();

app.UseExceptionHandler();
app.UseObservability();
app.UseHttpLogging();
app.UseForwardedHeaders();

app.MapDefaultEndpoints();
app.MapEndpoints();
app.MapOpenApi();

app.Run();
