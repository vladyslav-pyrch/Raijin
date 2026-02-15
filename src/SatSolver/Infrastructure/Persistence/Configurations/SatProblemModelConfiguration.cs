using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Raijin.SatSolver.Infrastructure.Persistence.Models;

namespace Raijin.SatSolver.Infrastructure.Persistence.Configurations;

public class SatProblemModelConfiguration : IEntityTypeConfiguration<SatProblemModel>
{
    public void Configure(EntityTypeBuilder<SatProblemModel> builder)
    {
        builder.HasKey(model => model.Id);

        builder.Property(model => model.Id).ValueGeneratedNever();

        builder.Property(model => model.Dimacs)
            .IsRequired();

        builder.PrimitiveCollection(model => model.Solution)
            .IsRequired();

        builder.Property(model => model.Satisfiability)
            .IsRequired()
            .HasMaxLength(20);
    }
}