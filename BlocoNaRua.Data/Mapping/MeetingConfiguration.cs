using BlocoNaRua.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlocoNaRua.Data.Mapping;

public class MeetingConfiguration : IEntityTypeConfiguration<MeetingEntity>
{
    public void Configure(EntityTypeBuilder<MeetingEntity> builder)
    {
        builder.ToTable("meetings");

        builder.HasKey(m => m.Id)
               .HasName("id");

        builder.Property(m => m.Id)
               .HasColumnName("id");

        builder.Property(m => m.CarnivalBlockId)
               .HasColumnName("carnival_block_id");

        builder.Property(m => m.Name)
               .HasColumnName("name");

        builder.Property(m => m.Description)
               .HasColumnName("description");

        builder.Property(m => m.Location)
               .HasColumnName("location");

        builder.Property(m => m.MeetingCode)
               .HasColumnName("meeting_code");

        builder.Property(m => m.MeetingDateTime)
               .HasColumnName("meeting_date_time");

        builder.Property(m => m.CreatedAt)
               .HasColumnName("created_at");

        builder.Property(m => m.UpdatedAt)
               .HasColumnName("updated_at");
               
        builder.HasOne(m => m.CarnivalBlock)
               .WithMany(cb => cb.CarnivalBlockMeetings)
               .HasForeignKey(m => m.CarnivalBlockId)
               .OnDelete(DeleteBehavior.Restrict)
               .HasConstraintName("meetings_carnival_block_id_fkey");

        // builder.HasMany(m => m.Presences)
        //        .WithOne(a => a.Meeting)
        //        .HasForeignKey(a => a.MeetingId);
    }
}
