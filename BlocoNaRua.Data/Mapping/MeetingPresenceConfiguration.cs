using BlocoNaRua.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlocoNaRua.Data.Mapping;

public class MeetingPresenceConfiguration : IEntityTypeConfiguration<MeetingPresenceEntity>
{
    public void Configure(EntityTypeBuilder<MeetingPresenceEntity> builder)
    {
        builder.Property(e => e.IsPresent)
               .IsRequired();

        builder.HasIndex(e => new { e.MeetingId, e.UserId })
               .IsUnique();
    }
}
