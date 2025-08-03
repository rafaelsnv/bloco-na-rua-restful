using BlocoNaRua.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlocoNaRua.Data.Mapping;

public class CarnivalBlockMembersConfiguration : IEntityTypeConfiguration<CarnivalBlockMembersEntity>
{
    public void Configure(EntityTypeBuilder<CarnivalBlockMembersEntity> builder)
    {
        builder.ToTable("carnival_block_members");

        builder.HasKey(e => e.Id)
               .HasName("id");

        builder.Property(e => e.Id)
               .HasColumnName("id")
               .IsRequired();

        builder.Property(e => e.CarnivalBlockId)
               .HasColumnName("carnival_block_id")
               .IsRequired();

        builder.Property(e => e.MemberId)
               .HasColumnName("member_id")
               .IsRequired();

        builder.Property(e => e.Role)
               .HasColumnName("role")
               .IsRequired()
               .HasConversion<string>();

        builder.Property(e => e.CreatedAt)
               .HasColumnName("created_at")
               .IsRequired();

        builder.Property(e => e.UpdatedAt)
               .HasColumnName("updated_at")
               .IsRequired(false);

        builder.HasOne(e => e.CarnivalBlock)
               .WithMany(cb => cb.CarnivalBlockMembers)
               .HasForeignKey(e => e.CarnivalBlockId)
               .OnDelete(DeleteBehavior.Cascade)
               .HasConstraintName("carnival_block_users_carnival_block_id_fkey");

        builder.HasOne(e => e.Member)
               .WithMany(u => u.CarnivalBlockMembers)
               .HasForeignKey(e => e.MemberId)
               .OnDelete(DeleteBehavior.Restrict)
               .HasConstraintName("carnival_block_users_user_id_fkey");
    }
}
