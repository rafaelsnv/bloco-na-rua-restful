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

        builder.Property(u => u.IsActive)
               .HasColumnName("is_active")
               .IsRequired()
               .HasDefaultValue(true);

        builder.Property(u => u.Provider)
               .HasColumnName("provider")
               .IsRequired(false);

        builder.Property(u => u.ProviderId)
               .HasColumnName("provider_id")
               .IsRequired(false);

        builder.Property(u => u.PasswordHash)
               .HasColumnName("password_hash")
               .IsRequired(false);

        builder.Property(u => u.RefreshToken)
               .HasColumnName("refresh_token")
               .IsRequired(false);

        builder.Property(u => u.RefreshTokenExpiresAt)
               .HasColumnName("refresh_token_expires_at")
               .IsRequired(false);

        builder.Property(u => u.EmailConfirmed)
               .HasColumnName("email_confirmed")
               .IsRequired()
               .HasDefaultValue(false);

        builder.HasQueryFilter(u => u.IsActive);

        builder.HasIndex(u => u.Uuid).IsUnique();

        builder.HasIndex(u => new { u.Provider, u.ProviderId })
               .IsUnique()
               .HasDatabaseName("ux_members_provider");

        builder.HasIndex(u => u.RefreshToken)
               .IsUnique()
               .HasDatabaseName("ux_members_refresh_token")
               .HasFilter("refresh_token IS NOT NULL");
    }
}
