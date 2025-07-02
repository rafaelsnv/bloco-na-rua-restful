using BlocoNaRua.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlocoNaRua.Data.Mapping;

public class MeetingPresenceConfiguration : IEntityTypeConfiguration<MeetingPresenceEntity>
{
    public void Configure(EntityTypeBuilder<MeetingPresenceEntity> builder)
    {
        builder.ToTable("meeting_presences");

        builder.HasKey(mp => mp.Id)
               .HasName("id");

        builder.Property(mp => mp.Id)
               .HasColumnName("id");

        builder.Property(mp => mp.MemberId)
               .HasColumnName("member_id")
               .IsRequired();

        builder.Property(mp => mp.MeetingId)
               .HasColumnName("meeting_id")
               .IsRequired();

        builder.Property(mp => mp.CarnivalBlockId)
               .HasColumnName("carnival_block_id")
               .IsRequired();

        builder.Property(e => e.IsPresent)
               .HasColumnName("is_present")
               .IsRequired();

        builder.Property(mp => mp.CreatedAt)
               .HasColumnName("created_at")
               .IsRequired();

        builder.Property(mp => mp.UpdatedAt)
               .HasColumnName("updated_at")
               .IsRequired(false);

        builder.HasOne(mp => mp.Member)
               .WithMany(u => u.Presences)
               .HasForeignKey(mp => mp.MemberId)
               .OnDelete(DeleteBehavior.Restrict)
               .HasConstraintName("meeting_presence_user_id_fkey");

        builder.HasOne(mp => mp.Meeting)
               .WithMany(m => m.Presences)
               .HasForeignKey(mp => mp.MeetingId)
               .OnDelete(DeleteBehavior.Restrict)
               .HasConstraintName("meeting_presence_meeting_id_fkey");

        builder.HasOne(mp => mp.CarnivalBlock)
               .WithMany(cb => cb.Presences)
               .HasForeignKey(mp => mp.CarnivalBlockId)
               .OnDelete(DeleteBehavior.Restrict)
               .HasConstraintName("meeting_presence_carnival_block_id_fkey");
    }
}
