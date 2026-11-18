using BlocoNaRua.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlocoNaRua.Data.Mapping;

public class ApiErrorConfiguration : IEntityTypeConfiguration<ApiErrorEntity>
{
    public void Configure(EntityTypeBuilder<ApiErrorEntity> builder)
    {
        builder.ToTable("api_errors");

        builder.HasKey(u => u.Id)
               .HasName("id");

        builder.Property(u => u.Id)
               .HasColumnName("id")
               .IsRequired();

        builder.Property(u => u.Level)
               .HasColumnName("level")
               .IsRequired();

        builder.Property(u => u.Source)
               .HasColumnName("source")
               .IsRequired(false);

        builder.Property(u => u.Message)
               .HasColumnName("message")
               .IsRequired();

        builder.Property(u => u.StackTrace)
               .HasColumnName("stack_trace")
               .IsRequired(false);

        builder.Property(u => u.RequestPath)
               .HasColumnName("request_path")
               .IsRequired(false);

        builder.Property(u => u.RequestMethod)
               .HasColumnName("request_method")
               .IsRequired(false);

        builder.Property(u => u.StatusCode)
               .HasColumnName("status_code")
               .IsRequired(false);

        builder.Property(u => u.UserId)
               .HasColumnName("user_id")
               .IsRequired(false);

        builder.Property(u => u.AdditionalData)
               .HasColumnName("additional_data")
               .HasColumnType("jsonb")
               .IsRequired(false);

        builder.Property(u => u.CreatedAt)
               .HasColumnName("created_at")
               .IsRequired();

        // The existing api_errors table has no updated_at column; ignore the base property.
        builder.Ignore(u => u.UpdatedAt);
    }
}
