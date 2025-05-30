using BlocoNaRua.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlocoNaRua.Data.Mapping;

public class UserConfiguration : IEntityTypeConfiguration<UserEntity>
{
    public void Configure(EntityTypeBuilder<UserEntity> builder)
    {
        // builder.ToTable("Users"); // TODO

        builder.HasKey(u => u.Id);

        builder.HasMany(u => u.CarnivalBlockUsers)
               .WithOne(cbu => cbu.User)
               .HasForeignKey(cbu => cbu.UserId);

        builder.HasMany(u => u.MeetingAttendances)
               .WithOne(a => a.User)
               .HasForeignKey(a => a.UserId);
    }
}
