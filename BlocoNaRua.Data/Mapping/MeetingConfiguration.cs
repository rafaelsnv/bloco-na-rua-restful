using BlocoNaRua.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlocoNaRua.Data.Mapping;

public class MeetingConfiguration : IEntityTypeConfiguration<MeetingEntity>
{
    public void Configure(EntityTypeBuilder<MeetingEntity> builder)
    {
        // builder.ToTable("Meetings"); // TODO

        builder.HasKey(m => m.Id);

        builder.HasMany(m => m.Attendances)
               .WithOne(a => a.Meeting)
               .HasForeignKey(a => a.MeetingId);
    }
}
