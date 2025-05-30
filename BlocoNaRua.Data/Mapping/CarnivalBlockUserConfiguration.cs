using BlocoNaRua.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlocoNaRua.Data.Mapping;

public class CarnivalBlockUserConfiguration : IEntityTypeConfiguration<CarnivalBlockUserEntity>
{
    public void Configure(EntityTypeBuilder<CarnivalBlockUserEntity> builder)
    {
        // builder.ToTable("CarnivalBlockUsers"); // TODO

        builder.HasKey();

        builder.Property(e => e.Role)
               .IsRequired()
               .HasMaxLength(50);

        builder.HasIndex(e => new { e.CarnivalBlockId, e.UserId, e.Role })
               .IsUnique();
    }
}
