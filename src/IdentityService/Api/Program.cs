WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.AddServiceDefaults();

// Error handling
builder.Services.AddProblemDetails();

// Logging
builder.Services.AddHttpLogging();

// Security
builder.Services.AddAuthentication();
builder.Services.AddAuthorization();

// OpenAPI (optional but useful for internal tooling)
builder.Services.AddOpenApi();

// Modules registration
// builder.Services.AddInfrastructure();
// builder.Services.AddApplication();

WebApplication app = builder.Build();

// Error pipeline
app.UseExceptionHandler();

// Logging
app.UseHttpLogging();

// Zero-trust verification
app.UseAuthentication();
app.UseAuthorization();

// Service endpoints
app.MapDefaultEndpoints();
// app.MapEndpoints();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.Run();