using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Raijin.CombinatoricsService.Infrastructure.Persistence.Models;

namespace Raijin.CombinatoricsService.Infrastructure.Persistence.Configurations;

internal class BooleanProblemModelConfiguration : IEntityTypeConfiguration<BooleanProblemModel>
{
    public void Configure(EntityTypeBuilder<BooleanProblemModel> builder)
    {
        builder.HasKey(model => model.Id);

        builder.Property(model => model.Id).ValueGeneratedNever();

        builder.Property(model => model.Formula)
            .IsRequired();

        builder.Property(model => model.Satisfiability)
            .IsRequired();

        builder.OwnsMany(model => model.Solution, navigationBuilder =>
        {
            navigationBuilder.WithOwner()
                .HasForeignKey(model => model.BooleanProblemId);

            navigationBuilder.HasKey(model => new { model.BooleanProblemId, model.Id });

            navigationBuilder.Property(model => model.Id)
                .ValueGeneratedOnAdd();

            navigationBuilder.Property(model => model.VariableName)
                .IsRequired();

            navigationBuilder.Property(model => model.Value)
                .IsRequired();
        });
    }
}