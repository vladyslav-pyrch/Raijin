using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Raijin.CombinatoricsService.Infrastructure.Persistence;

namespace Raijin.CombinatoricsService.MigrationWorker;

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
        using Activity? activity = ActivitySource.StartActivity(nameof(MigrationWorker), ActivityKind.Client);

        try
        {
            logger.LogInformation("Starting CombinatoricsService database migration");

            using IServiceScope scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<CombinatoricsServiceDbContext>();
            
            await RunMigrationAsync(dbContext, cancellationToken);
            logger.LogInformation("CombinatoricsService database migration completed successfully");

            await SeedDataAsync(dbContext, cancellationToken);
            logger.LogInformation("CombinatoricsService database seeding completed successfully");
        }
        catch (Exception ex)
        {
            activity?.AddException(ex);
            logger.LogCritical(ex, "CombinatoricsService database migration failed");
            throw;
        }

        hostApplicationLifetime.StopApplication();
    }

    private static async Task RunMigrationAsync(DbContext dbContext, CancellationToken cancellationToken)
    {
        IExecutionStrategy strategy = dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            try
            {
                // Try normal migration first
                await dbContext.Database.MigrateAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                // Log the failure
                Console.WriteLine("Migration failed. Dropping and recreating database...");
                Console.WriteLine(ex);

                // Drop database
                await dbContext.Database.EnsureDeletedAsync(cancellationToken);

                // Recreate empty database
                await dbContext.Database.EnsureCreatedAsync(cancellationToken);

                // Apply migrations again
                await dbContext.Database.MigrateAsync(cancellationToken);
            }
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