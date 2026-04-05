using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Raijin.SatSolver.Infrastructure.Persistence.Models;

namespace Raijin.SatSolver.Infrastructure.Persistence.Configurations;

internal sealed class SatProblemModelConfiguration : IEntityTypeConfiguration<SatProblemModel>
{
    public void Configure(EntityTypeBuilder<SatProblemModel> builder)
    {
        builder.HasKey(model => model.Id);

        builder.Property(model => model.Id).ValueGeneratedNever();

        builder.PrimitiveCollection(model => model.Solution)
            .IsRequired();

        builder.Property(model => model.Satisfiability)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(model => model.SolvingStatus)
            .HasMaxLength(20)
            .IsRequired();

        builder.OwnsMany(model => model.Clauses, clauseBuilder =>
        {
            clauseBuilder.WithOwner()
                .HasForeignKey("SatProblemId");

            clauseBuilder.PrimitiveCollection(model => model.Literals)
                .IsRequired();
        });

        builder.Property(model => model.CreatedAt)
            .IsRequired();
    }
}