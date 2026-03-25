using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Raijin.CombinatoricsService.Infrastructure.Persistence.Models;

namespace Raijin.CombinatoricsService.Infrastructure.Persistence.Configurations;

internal class CombinatoricProblemModelConfiguration : IEntityTypeConfiguration<CombinatoricProblemModel>
{
    public void Configure(EntityTypeBuilder<CombinatoricProblemModel> builder)
    {
        builder.HasKey(model => model.Id);

        builder.Property(model => model.Id).ValueGeneratedNever();

        builder.PrimitiveCollection(model => model.Constraints)
            .IsRequired();

        builder.Property(model => model.Satisfiability)
            .IsRequired();

        builder.Property(model => model.Solution)
            .IsRequired()
            .HasColumnType("jsonb");

        builder.OwnsMany(model => model.DecisionVariables, navigationBuilder =>
        {
            navigationBuilder.WithOwner()
                .HasForeignKey(model => model.CombinatoricProblemId);

            navigationBuilder.HasKey(model => new { model.CombinatoricProblemId, model.Id });

            navigationBuilder.Property(model => model.Id)
                .ValueGeneratedOnAdd();

            navigationBuilder.Property(model => model.Name)
                .IsRequired();

            navigationBuilder.PrimitiveCollection(model => model.States)
                .IsRequired();
        });
    }
}