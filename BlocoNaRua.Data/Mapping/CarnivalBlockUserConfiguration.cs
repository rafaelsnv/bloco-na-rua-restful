using BlocoNaRua.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlocoNaRua.Data.Mapping;

public class CarnivalBlockUsersConfiguration : IEntityTypeConfiguration<CarnivalBlockUsersEntity>
{
    public void Configure(EntityTypeBuilder<CarnivalBlockUsersEntity> builder)
    {
        builder.ToTable("carnival_block_users");

        builder.HasKey(e => e.Id)
               .HasName("id");

        builder.Property(e => e.Id)
               .HasColumnName("id");

        builder.Property(e => e.CarnivalBlockId)
               .HasColumnName("carnival_block_id");

        builder.Property(e => e.UserId)
               .HasColumnName("user_id");

        builder.Property(e => e.Role)
               .HasColumnName("role");

        builder.Property(e => e.CreatedAt)
               .HasColumnName("created_at");

        builder.Property(e => e.UpdatedAt)
               .HasColumnName("updated_at");

        builder.HasOne(e => e.CarnivalBlock)
               .WithMany(cb => cb.CarnivalBlockUsers)
               .HasForeignKey(e => e.CarnivalBlockId)
               .OnDelete(DeleteBehavior.Restrict)
               .HasConstraintName("carnival_block_users_carnival_block_id_fkey");
        
        builder.HasOne(e => e.User)
               .WithMany(u => u.CarnivalBlockUsers)
               .HasForeignKey(e => e.UserId)
               .OnDelete(DeleteBehavior.Restrict)
               .HasConstraintName("carnival_block_users_user_id_fkey");
    }
}
