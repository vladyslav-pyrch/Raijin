using Azure.Provisioning.PostgreSql;
using Projects;
using Scalar.Aspire;

IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

builder.AddAzureContainerAppEnvironment("raijin-env");

var applicationDbServer = builder
    .AddAzurePostgresFlexibleServer("raijin-postgres")
    .ConfigureInfrastructure(config =>
    {
        PostgreSqlFlexibleServer flexibleServer = config.GetProvisionableResources()
            .OfType<PostgreSqlFlexibleServer>()
            .Single();
        
        flexibleServer.AvailabilityZone = "3";
        flexibleServer.HighAvailability = new PostgreSqlFlexibleServerHighAvailability
        {
            Mode = PostgreSqlFlexibleServerHighAvailabilityMode.Disabled
        };
        flexibleServer.Sku = new PostgreSqlFlexibleServerSku
        {
            Name = "Standard_B1ms",
            Tier = PostgreSqlFlexibleServerSkuTier.Burstable,
        };
        flexibleServer.Storage = new PostgreSqlFlexibleServerStorage
        {
            StorageSizeInGB = 32
        };
        flexibleServer.Backup = new PostgreSqlFlexibleServerBackupProperties
        {
            BackupRetentionDays = 7
        };
    })
    .RunAsContainer(resourceBuilder =>
    {
        resourceBuilder.WithLifetime(ContainerLifetime.Session)
            .WithImage("postgres:17.6")
            // .WithDataVolume("raijin-postgres-data")
            .WithPgWeb();
    });

var combinatoricsServiceDb = applicationDbServer
    .AddDatabase("raijin-comb-db");

var combinatoricsServiceMigrationWorker = builder
    .AddProject<Raijin_CombinatoricsService_MigrationWorker>("raijin-comb-migrate")
    .WithReference(combinatoricsServiceDb)
    .WaitFor(combinatoricsServiceDb)
    .PublishAsAzureContainerAppJob();

builder.AddDockerfile("raijin-comb-sat-solver", "../../", "./src/CombinatoricsService/SatSolver/Dockerfile")
    .WithOtlpExporter()
    .WithEnvironment("MAX_JOBS_COUNT", "3")
    .WithReference(combinatoricsServiceDb)
    .WaitFor(combinatoricsServiceDb)
    .WaitForCompletion(combinatoricsServiceMigrationWorker);

var combinatoricsServiceApi = builder
    .AddProject<Raijin_CombinatoricsService_Api>("raijin-comb-api")
    .WithHttpHealthCheck("/health")
    .WithReference(combinatoricsServiceDb)
    .WaitFor(combinatoricsServiceDb)
    .WaitForCompletion(combinatoricsServiceMigrationWorker);

builder.AddProject<Raijin_Bff>("raijin-bff")
    .WithHttpHealthCheck("/health")
    .WithExternalHttpEndpoints()
    .WithReference(combinatoricsServiceApi)
    .WaitFor(combinatoricsServiceApi);

builder.AddScalarApiReference(options => options.AllowSelfSignedCertificates = true)
    .WithApiReference(combinatoricsServiceApi, (options, _) =>
    {
        options.AddDocument("v1");
        return Task.CompletedTask;
    })
    .WaitFor(combinatoricsServiceApi)
    .WithLifetime(ContainerLifetime.Persistent)
    .ExcludeFromManifest();

builder.Build().Run();
