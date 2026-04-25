using Raijin.CombinatoricsService.Api.Extensions;
using Raijin.CombinatoricsService.Application;
using Raijin.CombinatoricsService.Infrastructure;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.AddServiceDefaults();

// Error handling
builder.Services.AddProblemDetails();

// Logging
builder.Services.AddHttpLogging(options =>
{
    options.CombineLogs = true;
});

// Edge policies
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(cors =>
    {
        cors.WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>())
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
builder.Services.AddRateLimiter();

// Security
builder.Services.AddAuthentication();
builder.Services.AddAuthorization();

// Response optimization
builder.Services.AddResponseCompression();
builder.Services.AddResponseCaching();


// Api endpoints
builder.Services.AddOpenApi();
builder.Services.AddEndpoints();

// Modules registration
builder.Services.AddInfrastructure();
builder.Services.AddApplication();

WebApplication app = builder.Build();

// Error pipeline
app.UseExceptionHandler();

// Logging
app.UseHttpLogging();

// Edge security
app.UseHsts();
app.UseHttpsRedirection();

// Policies
app.UseRateLimiter();
app.UseCors();

// Zero-trust verification
app.UseAuthentication();
app.UseAuthorization();

// Response handling
app.UseResponseCompression();
app.UseResponseCaching();

// Service endpoints
app.MapDefaultEndpoints();
app.MapEndpoints();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.Run();