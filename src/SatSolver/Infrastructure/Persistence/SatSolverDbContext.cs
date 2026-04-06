using MassTransit;
using Microsoft.EntityFrameworkCore;
using Raijin.SatSolver.Infrastructure.Persistence.Models;

namespace Raijin.SatSolver.Infrastructure.Persistence;

public sealed class SatSolverDbContext(DbContextOptions<SatSolverDbContext> options) : DbContext(options)
{
    internal DbSet<SatProblemModel> SatProblems { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(InfrastructureModule.Assembly);

        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();
    }
}