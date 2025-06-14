using BlocoNaRua.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlocoNaRua.Data.Mapping;

public class UserConfiguration : IEntityTypeConfiguration<UserEntity>
{
    public void Configure(EntityTypeBuilder<UserEntity> builder)
    {
        builder.ToTable("users");

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

        builder.Property(u => u.Password)
               .HasColumnName("password")
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
    }
}
