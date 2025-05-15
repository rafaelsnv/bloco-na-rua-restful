using BlocoNaRua.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlocoNaRua.Data.Mapping;

public class CarnivalBlockConfiguration : IEntityTypeConfiguration<CarnivalBlockEntity>
{
    public void Configure(EntityTypeBuilder<CarnivalBlockEntity> builder)
    {
        // builder.ToTable("CarnivalBlocks"); // TODO
        builder.HasKey(cb => cb.Id);

        builder.HasMany(cb => cb.Meetings)
               .WithOne()
               .HasForeignKey(m => m.CarnivalBlockId);

        builder.HasMany(cb => cb.CarnivalBlockUsers)
               .WithOne(cbu => cbu.CarnivalBlock)
               .HasForeignKey(cbu => cbu.CarnivalBlockId);
    }
}
