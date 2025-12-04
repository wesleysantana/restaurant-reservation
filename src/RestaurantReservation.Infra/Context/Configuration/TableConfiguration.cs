using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestaurantReservation.Domain.Entities;
using RestaurantReservation.Domain.ValueObjects;

namespace RestaurantReservation.Infra.Context.Configuration;

public class TableConfiguration : IEntityTypeConfiguration<Table>
{
    public void Configure(EntityTypeBuilder<Table> builder)
    {
        builder.ToTable("tables");

        // Chave primária com valor padrão
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("gen_random_uuid()")
            .IsRequired();

        // Value Objects
        builder.OwnsOne(t => t.Name, name =>
        {
            name.WithOwner()
                .HasForeignKey("Id");

            name.Property(n => n.Value)
                .HasColumnName("name")
                .HasMaxLength(100)
                .IsRequired();

            // Índice no VO
            name.HasIndex(n => n.Value)
                .IsUnique()
                .HasDatabaseName("ix_tables_name_unique");
        });      

        builder.OwnsOne(t => t.Capacity, capacity =>
        {
            capacity.WithOwner()
                   .HasForeignKey("Id");

            capacity.Property(c => c.Value)
                .HasColumnName("capacity")
                .IsRequired();
        });

        builder.Property(t => t.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();
        
        builder.Property(t => t.Active)
            .HasColumnName("active")
            .IsRequired();
       
        builder.Property(t => t.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("now()")
            .IsRequired();     
        
        builder.HasIndex(t => t.Status)
            .HasDatabaseName("ix_tables_status");
        builder.HasIndex(t => t.Active)
            .HasDatabaseName("ix_tables_active");
    }
}