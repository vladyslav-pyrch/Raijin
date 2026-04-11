using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Raijin.CombinatoricsService.Infrastructure.Persistence.Models;

namespace Raijin.CombinatoricsService.Infrastructure.Persistence.Configurations;

internal sealed class SatRunModelConfiguration : IEntityTypeConfiguration<SatRunModel>
{
    public void Configure(EntityTypeBuilder<SatRunModel> builder)
    {
        builder.ToTable("SatRuns");

        builder.HasKey(run => run.Id);

        builder.Property(run => run.Satisfiability)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(run => run.Status)
            .IsRequired()
            .HasMaxLength(50);

        builder.PrimitiveCollection(run => run.Assignment);

        builder.Property(run => run.CreatedAt)
            .IsRequired();
        
        builder.HasOne<ProblemModel>()
            .WithOne()
            .HasForeignKey<SatRunModel>(run => run.ProblemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.OwnsOne(run => run.SatEncoding, satEncoding =>
        {
            satEncoding.ToTable("SatEncodings");

            satEncoding.HasKey(encoding => encoding.Id);

            satEncoding.WithOwner()
                .HasForeignKey(encoding => encoding.ProblemId);

            satEncoding.Property(encoding => encoding.Id)
                .ValueGeneratedOnAdd();

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
    }
}
