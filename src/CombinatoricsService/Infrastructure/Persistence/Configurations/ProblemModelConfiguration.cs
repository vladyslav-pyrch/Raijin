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

        builder.Property(problem => problem.Instance)
            .HasColumnType("jsonb");

        builder.Property(problem => problem.Solution)
            .HasColumnType("jsonb");
        
        builder.HasOne<SatRunModel>()
            .WithOne()
            .HasForeignKey<ProblemModel>(problem => problem.SatRunId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}