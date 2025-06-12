using BlocoNaRua.Data.Mapping;
using BlocoNaRua.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BlocoNaRua.Data.Context;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<UserEntity> Users { get; set; }
    public DbSet<MeetingEntity> Meetings { get; set; }
    public DbSet<CarnivalBlockEntity> CarnivalBlocks { get; set; }
    public DbSet<MeetingPresenceEntity> MeetingAttendances { get; set; }
    public DbSet<CarnivalBlockUserEntity> CarnivalBlockUsers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasDefaultSchema("public")
            .ApplyConfiguration(new UserConfiguration())
            .ApplyConfiguration(new CarnivalBlockConfiguration());

        base.OnModelCreating(modelBuilder);
    }
}
