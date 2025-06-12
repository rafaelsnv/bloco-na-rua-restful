using BlocoNaRua.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlocoNaRua.Data.Mapping;

public class CarnivalBlockConfiguration : IEntityTypeConfiguration<CarnivalBlockEntity>
{
    public void Configure(EntityTypeBuilder<CarnivalBlockEntity> builder)
    {
        builder.ToTable("carnival_blocks");

        builder.HasKey(cb => cb.Id)
               .HasName("id");

        builder.Property(u => u.Id)
               .HasColumnName("id");

        builder.Property(u => u.Name)
                .HasColumnName("name");

        builder.Property(u => u.Owner)
               .HasColumnName("owner");

        builder.Property(u => u.InviteCode)
               .HasColumnName("invite_code");

        builder.Property(u => u.ManagersInviteCode)
               .HasColumnName("managers_invite_code");

        builder.Property(u => u.CarnivalBlockImage)
               .HasColumnName("carnival_block_image");

        builder.Property(u => u.CreatedAt)
               .HasColumnName("created_at");
        
        builder.Property(u => u.UpdatedAt)
               .HasColumnName("updated_at");

        // builder.HasMany(cb => cb.Meetings)
        //        .WithOne()
        //        .HasForeignKey(m => m.CarnivalBlockId);

        // builder.HasMany(cb => cb.CarnivalBlockUsers)
        //        .WithOne(cbu => cbu.CarnivalBlock)
        //        .HasForeignKey(cbu => cbu.CarnivalBlockId);
    }
}
