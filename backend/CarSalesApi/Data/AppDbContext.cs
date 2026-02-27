using CarSalesApi.Cars;
using Microsoft.EntityFrameworkCore;

namespace CarSalesApi.Data;

public class AppDbContext : DbContext
{
    public DbSet<Car> Cars { get; set; }
    public DbSet<CarHistory> CarsHistory { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=cars.sqlite");
        optionsBuilder.LogTo(Console.WriteLine, LogLevel.Information);
        optionsBuilder.EnableSensitiveDataLogging();
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Criando PKs
        modelBuilder.Entity<Car>().HasKey(c => c.Id);
        modelBuilder.Entity<CarHistory>().HasKey(c => c.Id);
        
        // Criando FK CarHistory.CarId -> Car.Id
        modelBuilder.Entity<CarHistory>()
            .HasOne(ch => ch.Car)
            .WithMany(c => c.History)
            .HasForeignKey(ch => ch.CarId);
        
        base.OnModelCreating(modelBuilder);
    }
}