using System.Diagnostics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using OpenTelemetryBuilderOtlpExporterExtensions = OpenTelemetry.OpenTelemetryBuilderOtlpExporterExtensions;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.Hosting;

// Adds common .NET Aspire services: service discovery, resilience, health checks, and OpenTelemetry.
// This project should be referenced by each service project in your solution.
// To learn more about using this project, see https://aka.ms/dotnet/aspire/service-defaults
public static class Extensions
{
    private const string HealthEndpointPath = "/health";
    private const string AlivenessEndpointPath = "/alive";

    public static TBuilder AddServiceDefaults<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.Services.AddLogging(loggingBuilder =>
        {
            loggingBuilder.AddConfiguration();
        });

        builder.ConfigureOpenTelemetry();

        builder.AddDefaultHealthChecks();

        builder.Services.AddServiceDiscovery();

        builder.Services.ConfigureHttpClientDefaults(http =>
        {
            // Turn on resilience by default
            http.AddStandardResilienceHandler();

            // Turn on service discovery by default
            http.AddServiceDiscovery();
        });

        // Uncomment the following to restrict the allowed schemes for service discovery.
        // builder.Services.Configure<ServiceDiscoveryOptions>(options =>
        // {
        //     options.AllowedSchemes = ["https"];
        // });

        return builder;
    }

    public static WebApplication UseObservability(this WebApplication app)
    {
        ILogger logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Raijin.Startup");
        bool useOtlpExporter = !string.IsNullOrWhiteSpace(app.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);

        logger.LogInformation(
            "Service starting. ServiceName={ServiceName} Environment={Environment} OtlpExporterEnabled={OtlpExporterEnabled} HealthEndpoint={HealthEndpoint} AlivenessEndpoint={AlivenessEndpoint} ServiceDiscoveryEnabled={ServiceDiscoveryEnabled} ResilienceEnabled={ResilienceEnabled}",
            app.Environment.ApplicationName,
            app.Environment.EnvironmentName,
            useOtlpExporter,
            HealthEndpointPath,
            AlivenessEndpointPath,
            true,
            true);

        app.Use(async (context, next) =>
        {
            string requestId = context.TraceIdentifier;
            Activity? activity = Activity.Current;
            string? traceId = activity?.TraceId.ToString();
            string? spanId = activity?.SpanId.ToString();

            using IDisposable? scope = app.Logger.BeginScope(new Dictionary<string, object?>
            {
                ["RequestId"] = requestId,
                ["TraceId"] = traceId,
                ["SpanId"] = spanId
            });

            var stopwatch = Stopwatch.StartNew();

            try
            {
                await next(context);
            }
            finally
            {
                stopwatch.Stop();

                if (!context.Request.Path.StartsWithSegments(HealthEndpointPath)
                    && !context.Request.Path.StartsWithSegments(AlivenessEndpointPath))
                {
                    app.Logger.LogInformation(
                        "HTTP request completed. Method={Method} Path={Path} StatusCode={StatusCode} ElapsedMs={ElapsedMs}",
                        context.Request.Method,
                        context.Request.Path.Value,
                        context.Response.StatusCode,
                        stopwatch.ElapsedMilliseconds);
                }
            }
        });

        return app;
    }

    public static IHost LogRaijinStartup(this IHost host)
    {
        ILogger logger = host.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Raijin.Startup");
        IHostEnvironment environment = host.Services.GetRequiredService<IHostEnvironment>();
        IConfiguration configuration = host.Services.GetRequiredService<IConfiguration>();
        bool useOtlpExporter = !string.IsNullOrWhiteSpace(configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);

        logger.LogInformation(
            "Service starting. ServiceName={ServiceName} Environment={Environment} OtlpExporterEnabled={OtlpExporterEnabled} HealthEndpoint={HealthEndpoint} AlivenessEndpoint={AlivenessEndpoint} ServiceDiscoveryEnabled={ServiceDiscoveryEnabled} ResilienceEnabled={ResilienceEnabled}",
            environment.ApplicationName,
            environment.EnvironmentName,
            useOtlpExporter,
            HealthEndpointPath,
            AlivenessEndpointPath,
            true,
            true);

        return host;
    }

    public static TBuilder ConfigureOpenTelemetry<TBuilder>(this TBuilder builder)
        where TBuilder : IHostApplicationBuilder
    {
        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
            logging.ParseStateValues = true;
        });

        builder.Services.AddOpenTelemetry()
            .WithMetrics(metrics =>
            {
                metrics.AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation();
            })
            .WithTracing(tracing =>
            {
                tracing.AddSource(builder.Environment.ApplicationName)
                    .AddAspNetCoreInstrumentation(options =>
                        // Exclude health check requests from tracing
                        options.Filter = context =>
                            !context.Request.Path.StartsWithSegments(HealthEndpointPath)
                            && !context.Request.Path.StartsWithSegments(AlivenessEndpointPath)
                    )
                    // Uncomment the following line to enable gRPC instrumentation (requires the OpenTelemetry.Instrumentation.GrpcNetClient package)
                    //.AddGrpcClientInstrumentation()
                    .AddHttpClientInstrumentation();
            });

        builder.AddOpenTelemetryExporters();

        return builder;
    }

    // ReSharper disable once UnusedMethodReturnValue.Local
    private static TBuilder AddOpenTelemetryExporters<TBuilder>(this TBuilder builder)
        where TBuilder : IHostApplicationBuilder
    {
        bool useOtlpExporter = !string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);

        if (useOtlpExporter)
            OpenTelemetryBuilderOtlpExporterExtensions.UseOtlpExporter(builder.Services.AddOpenTelemetry());

        // Uncomment the following lines to enable the Azure Monitor exporter (requires the Azure.Monitor.OpenTelemetry.AspNetCore package)
        // if (!string.IsNullOrEmpty(builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]))
        // {
        //     builder.Services.AddOpenTelemetry()
        //        .UseAzureMonitor();
        // }

        return builder;
    }

    public static TBuilder AddDefaultHealthChecks<TBuilder>(this TBuilder builder)
        where TBuilder : IHostApplicationBuilder
    {
        builder.Services.AddHealthChecks()
            // Add a default liveness check to ensure app is responsive
            .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);

        return builder;
    }

    public static WebApplication MapDefaultEndpoints(this WebApplication app)
    {
        app.MapHealthChecks(HealthEndpointPath);
        app.MapHealthChecks(AlivenessEndpointPath, new HealthCheckOptions
        {
            Predicate = r => r.Tags.Contains("live")
        });

        return app;
    }
}
