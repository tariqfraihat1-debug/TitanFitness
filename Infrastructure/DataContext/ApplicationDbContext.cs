using Microsoft.EntityFrameworkCore;
using TitanFitness.Domain.Branches;
using TitanFitness.Domain.CheckIns;
using TitanFitness.Domain.ClassSessions;
using TitanFitness.Domain.Members;
using TitanFitness.Domain.Memberships;
using TitanFitness.Domain.Plans;
using TitanFitness.Domain.Trainers;

namespace TitanFitness.Infrastructure.DataContext;

public sealed class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<Studio> Studios => Set<Studio>();
    public DbSet<Member> Members => Set<Member>();
    public DbSet<Plan> Plans => Set<Plan>();
    public DbSet<Membership> Memberships => Set<Membership>();
    public DbSet<Freeze> Freezes => Set<Freeze>();
    public DbSet<GuestPass> GuestPasses => Set<GuestPass>();
    public DbSet<Trainer> Trainers => Set<Trainer>();
    public DbSet<ClassSession> ClassSessions => Set<ClassSession>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<CheckIn> CheckIns => Set<CheckIn>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Make string comparisons case-insensitive across the database.
        modelBuilder.UseCollation("SQL_Latin1_General_CP1_CI_AS");

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}