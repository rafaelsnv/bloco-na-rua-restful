using BlocoNaRua.Data.Mapping;
using BlocoNaRua.Domain.Entities;
using BlocoNaRua.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace BlocoNaRua.Data.Context;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<MemberEntity> Members { get; set; }
    public DbSet<MeetingEntity> Meetings { get; set; }
    public DbSet<CarnivalBlockEntity> CarnivalBlocks { get; set; }
    public DbSet<MeetingPresenceEntity> MeetingAttendances { get; set; }
    public DbSet<CarnivalBlockMembersEntity> CarnivalBlockMembers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasDefaultSchema("public")
            .HasPostgresEnum<RolesEnum>()
            .ApplyConfiguration(new MemberConfiguration())
            .ApplyConfiguration(new MeetingConfiguration())
            .ApplyConfiguration(new CarnivalBlockConfiguration())
            .ApplyConfiguration(new MeetingPresenceConfiguration())
            .ApplyConfiguration(new CarnivalBlockMembersConfiguration());

        base.OnModelCreating(modelBuilder);
    }
}
