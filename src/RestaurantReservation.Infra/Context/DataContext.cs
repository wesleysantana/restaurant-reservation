using Microsoft.EntityFrameworkCore;
using RestaurantReservation.Domain.Entities;
using System.Reflection;

namespace RestaurantReservation.Infra.Context;

public class DataContext : DbContext
{
    public DataContext(DbContextOptions<DataContext> options) : base(options)
    {
    }

    public DbSet<Reservation> Reservations { get; set; }
    public DbSet<Table> Tables { get; set; }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder
            .Properties<string>()
            .HaveMaxLength(150);

        configurationBuilder
            .Properties<DateTime>()
            .HaveColumnType("timestamp");

        configurationBuilder
            .Properties<decimal>()
            .HavePrecision(18, 2);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Carrega todas as configurações específicas das entidades
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // Aplica as convenções de lowercase de forma global
        modelBuilder.ApplyLowerCaseNamingConvention();
    }    
}