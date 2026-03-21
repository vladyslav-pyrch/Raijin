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
    }
}