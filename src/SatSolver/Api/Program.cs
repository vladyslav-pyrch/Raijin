using Raijin.SatSolver.Api;
using Raijin.SatSolver.Application;
using Raijin.SatSolver.Infrastructure;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.AddServiceDefaults();
builder.Services.AddOpenApi();
builder.Services.AddEndpoints();
builder.Services.AddInfrastructureApi();
builder.Services.AddApplication();

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

await app.ApplyMigrations();

app.UseHttpsRedirection();

app.MapDefaultEndpoints();
app.MapEndpoints();

app.Run();
