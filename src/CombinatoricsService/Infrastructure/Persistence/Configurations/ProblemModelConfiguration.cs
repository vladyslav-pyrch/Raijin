using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Raijin.CombinatoricsService.Infrastructure.Persistence.Models;

namespace Raijin.CombinatoricsService.Infrastructure.Persistence.Configurations;

internal sealed class ProblemModelConfiguration : IEntityTypeConfiguration<ProblemModel>
{
    public void Configure(EntityTypeBuilder<ProblemModel> builder)
    {
        builder.HasKey(problem => problem.Id);

        builder.Property(problem => problem.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(problem => problem.Description)
            .IsRequired()
            .HasMaxLength(5000);

        builder.Property(problem => problem.ProblemType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(problem => problem.Solver)
            .HasMaxLength(100);

        builder.Property(problem => problem.Instance)
            .HasColumnType("json");

        builder.Property(problem => problem.Solution)
            .HasColumnType("json");

        builder.Property(problem => problem.SolvingStatus)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(problem => problem.Satisfiability)
            .IsRequired()
            .HasMaxLength(50);

        builder.PrimitiveCollection(problem => problem.Assignment);

        builder.Property(problem => problem.CreatedAt)
            .IsRequired();

        builder.Property(problem => problem.UpdatedAt)
            .IsRequired();
    }
}