using Raijin.SatSolver.Api;
using Raijin.SatSolver.Application;
using Raijin.SatSolver.Infrastructure;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.AddServiceDefaults();

// Security and static services
builder.Services.AddProblemDetails();
// builder.Services.AddHsts();
// builder.Services.AddHttpsRedirection();
// builder.Services.AddCookiePolicy();

// Policies services
builder.Services.AddRateLimiter();
// builder.Services.AddRequestLocalization();
builder.Services.AddCors();

// Authentication and session services
builder.Services.AddAuthentication();
builder.Services.AddAuthorization();
builder.Services.AddAntiforgery();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();

// Response services
builder.Services.AddResponseCompression();
builder.Services.AddResponseCaching();
builder.Services.AddEndpoints();
builder.Services.AddOpenApi();

builder.Services.AddInfrastructureApi();
builder.Services.AddApplication();

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.

// Security and static middlewares
app.UseExceptionHandler();
app.UseHsts();
app.UseHttpsRedirection();
app.UseCookiePolicy();

// Policies middlewares
app.UseRateLimiter();
app.UseRequestLocalization();
app.UseCors();

// Authentication and session middlewares
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();
app.UseSession();

// Response middlewares
app.UseResponseCompression();
app.UseResponseCaching();
app.MapDefaultEndpoints();
app.MapEndpoints();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

await app.ApplyMigrations();

app.Run();