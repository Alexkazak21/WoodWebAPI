using Microsoft.EntityFrameworkCore;
using WoodWebAPI.Data.Entities;

namespace WoodWebAPI.Data;

public partial class WoodDBContext : DbContext
{
    public WoodDBContext(DbContextOptions<WoodDBContext> options) : base(options)
    { }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderPosition> OrderPositions { get; set; }

    public virtual DbSet<EtalonTimber> EtalonTimberList { get; set; }

    public virtual DbSet<IsAdmin> IsAdmin { get; set; }

    

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.LogTo(Console.WriteLine, LogLevel.Information);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EtalonTimber>()
            .Property(x => x.LengthInMeter)
            .HasConversion(
                x => x.ToString(),
                x => decimal.Parse(x));

        modelBuilder.Entity<EtalonTimber>()
            .Property(x => x.DiameterInСantimeter)
            .HasConversion(
                x => x.ToString(),
                x => decimal.Parse(x));

        modelBuilder.Entity<OrderPosition>()
            .Property(x => x.LengthInMeter)
            .HasConversion(
                x => x.ToString(),
                x => decimal.Parse(x));

        modelBuilder.Entity<OrderPosition>()
            .Property(x => x.DiameterInCantimeter)
            .HasConversion(
                x => x.ToString(),
                x => decimal.Parse(x));

        base.OnModelCreating(modelBuilder);
    }
}
