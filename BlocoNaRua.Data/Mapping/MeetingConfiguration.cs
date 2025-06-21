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
               .HasColumnName("id")
               .IsRequired();

        builder.Property(m => m.CarnivalBlockId)
               .HasColumnName("carnival_block_id")
               .IsRequired();

        builder.Property(m => m.Name)
               .HasColumnName("name")
               .IsRequired(false);

        builder.Property(m => m.Description)
               .HasColumnName("description")
               .IsRequired(false);

        builder.Property(m => m.Location)
               .HasColumnName("location")
               .IsRequired(false);

        builder.Property(m => m.MeetingCode)
               .HasColumnName("meeting_code")
               .IsRequired(false);

        builder.Property(m => m.MeetingDateTime)
               .HasColumnName("meeting_date_time")
               .IsRequired(false);

        builder.Property(m => m.CreatedAt)
               .HasColumnName("created_at")
               .IsRequired();

        builder.Property(m => m.UpdatedAt)
               .HasColumnName("updated_at")
               .IsRequired(false);
               
        builder.HasOne(m => m.CarnivalBlock)
               .WithMany(cb => cb.Meetings)
               .HasForeignKey(m => m.CarnivalBlockId)
               .OnDelete(DeleteBehavior.Restrict)
               .HasConstraintName("meetings_carnival_block_id_fkey");
    }
}
