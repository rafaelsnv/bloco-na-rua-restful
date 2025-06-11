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
               .HasColumnName("id");

        builder.Property(u => u.Name)
                .HasColumnName("name");

        builder.Property(u => u.Email)
               .HasColumnName("email");

        builder.Property(u => u.Password)
               .HasColumnName("password");

        builder.Property(u => u.Phone)
               .HasColumnName("phone");

        builder.Property(u => u.ProfileImage)
               .HasColumnName("profile_image");

        builder.Property(u => u.CreatedAt)
               .HasColumnName("created_at");
        
        builder.Property(u => u.UpdatedAt)
               .HasColumnName("updated_at");

        // builder.HasMany(u => u.CarnivalBlockUsers) // TODO
        //        .WithOne(cbu => cbu.User)
        //        .HasForeignKey(cbu => cbu.UserId);

        // builder.HasMany(u => u.MeetingPresences) // TODO
        //        .WithOne(a => a.User)
        //        .HasForeignKey(a => a.UserId);
    }
}
