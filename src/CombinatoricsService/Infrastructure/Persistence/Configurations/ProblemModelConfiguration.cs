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

        builder.OwnsOne(problem => problem.SatEncoding, satEncoding =>
        {
            satEncoding.ToTable("SatEncodings");

            satEncoding.HasKey(encoding => encoding.Id);

            satEncoding.WithOwner()
                .HasForeignKey(encoding => encoding.ProblemId);

            satEncoding.Property(encoding => encoding.Id)
                .ValueGeneratedOnAdd();

            satEncoding.Property(encoding => encoding.VariableMap)
                .HasColumnType("jsonb")
                .IsRequired();

            satEncoding.OwnsMany(encoding => encoding.Clauses, clause =>
            {
                clause.ToTable("Clauses");

                clause.HasKey(c => c.Id);

                clause.WithOwner()
                    .HasForeignKey(c => c.SatEncodingId);

                clause.Property(c => c.Id)
                    .ValueGeneratedOnAdd();

                clause.PrimitiveCollection(c => c.Literals)
                    .IsRequired();
            });
        });

        builder.OwnsOne(problem => problem.SatRun, satRun =>
        {
            satRun.ToTable("SatRuns");

            satRun.HasKey(run => run.Id);

            satRun.WithOwner()
                .HasForeignKey(run => run.ProblemId);

            satRun.Property(run => run.Id)
                .ValueGeneratedOnAdd();

            satRun.Property(run => run.Satisfiability)
                .IsRequired()
                .HasMaxLength(50);

            satRun.Property(run => run.Status)
                .IsRequired()
                .HasMaxLength(50);

            satRun.PrimitiveCollection(run => run.Assignment);

            satRun.Property(run => run.CreatedAt)
                .IsRequired();
        });
    }
}