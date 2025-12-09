using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestaurantReservation.Domain.Entities;

namespace RestaurantReservation.Infra.Data.Configurations;

public class BusinessHoursRuleConfiguration : IEntityTypeConfiguration<BusinessHoursRule>
{
    public void Configure(EntityTypeBuilder<BusinessHoursRule> builder)
    {
        builder.ToTable("business_hours_rules");       

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("gen_random_uuid()")
            .IsRequired();        

        builder.Property(r => r.StartDate)
            .HasColumnName("start_date")
            .HasConversion(
                v => v.ToString("yyyy-MM-dd"),
                v => DateOnly.Parse(v))
            .IsRequired();

        builder.Property(r => r.EndDate)
            .HasColumnName("end_date")
            .HasConversion(
                v => v.ToString("yyyy-MM-dd"),
                v => DateOnly.Parse(v))
            .IsRequired();       

        builder.Property(r => r.SpecificDate)
            .HasColumnName("specific_date")
            .HasConversion(
                v => v.HasValue ? v.Value.ToString("yyyy-MM-dd") : null,
                v => v != null ? DateOnly.Parse(v) : null);

        builder.Property(r => r.WeekDay)
            .HasColumnName("week_day")
            .HasConversion<string>()
            .HasMaxLength(20)   
            .IsRequired(false);

        builder.Property(r => r.Open)
            .HasColumnName("open_time")
            .HasConversion(
                v => v.HasValue ? v.Value.ToString("HH:mm:ss") : null,
                v => v != null ? TimeOnly.Parse(v) : null);

        builder.Property(r => r.Close)
            .HasColumnName("close_time")
            .HasConversion(
                v => v.HasValue ? v.Value.ToString("HH:mm:ss") : null,
                v => v != null ? TimeOnly.Parse(v) : null);

        builder.Property(r => r.IsClosed)
            .HasColumnName("is_closed")
            .IsRequired();       

        builder.Property(r => r.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("now()")
            .IsRequired();        

        builder.HasIndex(r => r.StartDate)
            .HasDatabaseName("ix_business_hours_start_date");

        builder.HasIndex(r => r.EndDate)
            .HasDatabaseName("ix_business_hours_end_date");

        builder.HasIndex(r => r.SpecificDate)
            .HasDatabaseName("ix_business_hours_specific_date");

        builder.HasIndex(r => r.WeekDay)
            .HasDatabaseName("ix_business_hours_day_of_week");
    }
}
