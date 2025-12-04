using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestaurantReservation.Domain.Entities;

namespace RestaurantReservation.Infra.Context.Configuration;

public class ReservationConfiguration : IEntityTypeConfiguration<Reservation>
{
    public void Configure(EntityTypeBuilder<Reservation> builder)
    {
        builder.ToTable("reservations");

        // Chave primária com valor padrão
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("gen_random_uuid()")
            .IsRequired();

        // Foreign Keys
        builder.Property(r => r.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(r => r.TableId)
            .HasColumnName("table_id")
            .IsRequired();

        // Datas de início e fim
        builder.Property(r => r.StartsAt)
            .HasColumnName("starts_at")
            .IsRequired();

        builder.Property(r => r.EndsAt)
            .HasColumnName("ends_at")
            .IsRequired();

        // Status - as constraints CHECK serão criadas via SQL na migration
        builder.Property(r => r.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        // CreatedAt (timestamp padrão)
        builder.Property(r => r.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("now()")
            .IsRequired();

        // Relacionamentos
        builder.HasOne<Table>()
            .WithMany()
            .HasForeignKey(r => r.TableId)
            .OnDelete(DeleteBehavior.Restrict);

        // Índices
        builder.HasIndex(r => r.UserId)
            .HasDatabaseName("ix_reservations_user_id");

        builder.HasIndex(r => r.TableId)
            .HasDatabaseName("ix_reservations_table_id");

        builder.HasIndex(r => r.Status)
            .HasDatabaseName("ix_reservations_status");

        builder.HasIndex(r => r.StartsAt)
            .HasDatabaseName("ix_reservations_starts_at");

        // Índice composto para busca por mesa e período
        builder.HasIndex(r => new { r.TableId, r.StartsAt, r.EndsAt })
            .HasDatabaseName("idx_reservations_table_time");
    }
}