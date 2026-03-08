WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.AddServiceDefaults();

// Error handling
builder.Services.AddProblemDetails();

// Edge policies
builder.Services.AddCors();
builder.Services.AddRateLimiter();

// Security
builder.Services.AddAuthentication();
builder.Services.AddAuthorization();

// Response optimization
builder.Services.AddResponseCompression();
builder.Services.AddResponseCaching();

// OpenAPI aggregation if used
builder.Services.AddOpenApi();

WebApplication app = builder.Build();

// Error handling
app.UseExceptionHandler();

// Edge security
app.UseHsts();
app.UseHttpsRedirection();

// Policies
app.UseRateLimiter();
app.UseCors();

// Auth
app.UseAuthentication();
app.UseAuthorization();

// Response handling
app.UseResponseCompression();
app.UseResponseCaching();

// Gateway routing
app.MapDefaultEndpoints();
// app.MapReverseProxy();   // typical if using YARP

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.Run();