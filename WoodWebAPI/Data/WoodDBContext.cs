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
        modelBuilder.Entity<Order>()
             .HasOne(x => x.Customer)
             .WithMany(x => x.Orders)
             .HasPrincipalKey(x => x.TelegramID);

        modelBuilder.Entity<EtalonTimber>()
            .Property(x => x.LengthInMeter)
            .HasConversion(
            x => Convert.ToDouble(x),
            x => Convert.ToDecimal(x)
            );

        modelBuilder.Entity<EtalonTimber>()
            .Property(x => x.DiameterInСantimeter)
            .HasConversion(
            x => Convert.ToDouble(x),
            x => Convert.ToDecimal(x)
            );
        modelBuilder.Entity<OrderPosition>()
            .Property(x => x.LengthInMeter)
            .HasConversion(
            x => Convert.ToDouble(x),
            x => Convert.ToDecimal(x)
            );

        modelBuilder.Entity<OrderPosition>()
            .Property(x => x.DiameterInCantimeter)
            .HasConversion(
            x => Convert.ToDouble(x),
            x => Convert.ToDecimal(x)
            );

        base.OnModelCreating(modelBuilder);
    }
}
