using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Raijin.QueryService.MigrationWorker;

public class MigrationWorker(
    IServiceProvider serviceProvider,
    IHostApplicationLifetime hostApplicationLifetime,
    ILogger<MigrationWorker> logger
) : BackgroundService
{
    public const string ActivitySourceName = nameof(MigrationWorker);
    private static readonly ActivitySource ActivitySource = new(ActivitySourceName);

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        using Activity activity = ActivitySource.StartActivity("Migrating database", ActivityKind.Client);

        try
        {
            logger.LogInformation("Starting QueryService database migration");

            using IServiceScope scope = serviceProvider.CreateScope();
            // var dbContext = scope.ServiceProvider.GetRequiredService<DbContext>();
            //
            // await RunMigrationAsync(dbContext, cancellationToken);
            // logger.LogInformation("QueryService database migration completed successfully");
            //
            // await SeedDataAsync(dbContext, cancellationToken);
            // logger.LogInformation("QueryService database seeding completed successfully");
        }
        catch (Exception ex)
        {
            activity?.AddException(ex);
            logger.LogError(ex, "QueryService database migration failed");
            throw;
        }

        hostApplicationLifetime.StopApplication();
    }

    private static async Task RunMigrationAsync(DbContext dbContext, CancellationToken cancellationToken)
    {
        IExecutionStrategy strategy = dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            // Run migration in a transaction to avoid partial migration if it fails.
            await dbContext.Database.MigrateAsync(cancellationToken);
            // await dbContext.Database.EnsureCreatedAsync(cancellationToken);
        });
    }

    private static async Task SeedDataAsync(DbContext dbContext, CancellationToken cancellationToken)
    {
        IExecutionStrategy strategy = dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            // Seed the database
            await using IDbContextTransaction transaction = await dbContext.Database
                .BeginTransactionAsync(cancellationToken);

            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        });
    }
}
