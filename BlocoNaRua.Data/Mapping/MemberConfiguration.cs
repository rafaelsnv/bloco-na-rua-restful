using BlocoNaRua.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlocoNaRua.Data.Mapping;

public class MemberConfiguration : IEntityTypeConfiguration<MemberEntity>
{
    public void Configure(EntityTypeBuilder<MemberEntity> builder)
    {
        builder.ToTable("members");

        builder.HasKey(u => u.Id)
               .HasName("id");

        builder.Property(u => u.Id)
               .HasColumnName("id")
               .IsRequired();

        builder.Property(u => u.Name)
                .HasColumnName("name")
                .IsRequired();

        builder.Property(u => u.Email)
               .HasColumnName("email")
               .IsRequired();

        builder.Property(u => u.Phone)
               .HasColumnName("phone")
               .IsRequired();

        builder.Property(u => u.ProfileImage)
               .HasColumnName("profile_image")
               .IsRequired(false);

        builder.Property(u => u.CreatedAt)
               .HasColumnName("created_at")
               .IsRequired();

        builder.Property(u => u.UpdatedAt)
               .HasColumnName("updated_at")
               .IsRequired(false);

        builder.Property(u => u.Uuid)
               .HasColumnName("uuid")
               .IsRequired();

        builder.HasIndex(u => u.Uuid).IsUnique();
    }
}
