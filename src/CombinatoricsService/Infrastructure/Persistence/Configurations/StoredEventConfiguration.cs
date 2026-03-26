using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Raijin.CombinatoricsService.Infrastructure.Persistence.Models;

namespace Raijin.CombinatoricsService.Infrastructure.Persistence.Configurations;

public class StoredEventConfiguration : IEntityTypeConfiguration<StoredEvent>
{
    public void Configure(EntityTypeBuilder<StoredEvent> builder)
    {
        builder.HasKey(e => e.Id);

        builder.HasIndex(e => e.StreamId);

        builder.Property(e => e.Id)
            .ValueGeneratedOnAdd();

        builder.Property(e => e.StreamId)
            .IsRequired();

        builder.Property(e => e.AggregateType)
            .IsRequired()
            .HasMaxLength(512);

        builder.Property(e => e.EventType)
            .IsRequired()
            .HasMaxLength(512);

        builder.Property(e => e.EventData)
            .IsRequired()
            .HasColumnType("jsonb");

        builder.Property(e => e.Timestamp)
            .IsRequired();
    }
}