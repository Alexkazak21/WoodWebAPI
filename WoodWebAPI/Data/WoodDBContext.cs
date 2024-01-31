using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WoodWebAPI.Data.Entities;
using WoodWebAPI.Auth;

namespace WoodWebAPI.Data;

public partial class WoodDBContext : DbContext
{
    public WoodDBContext(DbContextOptions<WoodDBContext> options) : base(options)
    { }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<Timber> Timbers { get; set; }

    public virtual DbSet<Kubs> Kubs { get; set; }

    public virtual DbSet<IsAdmin> IsAdmin { get; set; }

    

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.LogTo(Console.WriteLine, LogLevel.Information);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}
