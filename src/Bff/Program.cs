using Yarp.ReverseProxy.Configuration;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddProblemDetails();
builder.Services.AddHttpLogging(options => options.CombineLogs = true);
builder.Services.AddCors();
builder.Services.AddReverseProxy()
    .LoadFromMemory(
        [
            new RouteConfig
            {
                RouteId = "combinatorics-api",
                ClusterId = "combinatorics-api",
                Match = new RouteMatch
                {
                    Path = "/api/combinatorics/{**catch-all}"
                },
                Transforms =
                [
                    new Dictionary<string, string>
                    {
                        ["PathRemovePrefix"] = "/api/combinatorics"
                    },
                    new Dictionary<string, string>
                    {
                        ["X-Forwarded"] = "Set",
                        ["HeaderPrefix"] = "X-Forwarded-",
                    }
                ]
            }
        ],
        [
            new ClusterConfig
            {
                ClusterId = "combinatorics-api",
                Destinations = new Dictionary<string, DestinationConfig>
                {
                    ["destination1"] = new()
                    {
                        Address = "http://raijin-comb-api"
                    }
                }
            }
        ])
    .AddServiceDiscoveryDestinationResolver();

WebApplication app = builder.Build();

app.UseExceptionHandler();
app.UseHttpLogging();
app.UseCors();
app.UseHsts();
app.UseHttpsRedirection();
app.UseForwardedHeaders();
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapDefaultEndpoints();
app.MapReverseProxy();
app.MapFallbackToFile("index.html");

app.Run();

