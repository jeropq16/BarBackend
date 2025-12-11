using Barber.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Barber.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    
    public DbSet<User> Users { get; set; }
    public DbSet<HairCut> HairCuts { get; set; }
    public DbSet<Appointment> Appointments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>().ToTable("Users");
        modelBuilder.Entity<HairCut>().ToTable("HairCuts");
        modelBuilder.Entity<Appointment>().ToTable("Appointments");
    }
}