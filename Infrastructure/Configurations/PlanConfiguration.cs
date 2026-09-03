using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TitanFitness.Domain.Common.Entities;
using TitanFitness.Domain.Plans;

namespace TitanFitness.Infrastructure.Configurations;

public sealed class PlanConfiguration : IEntityTypeConfiguration<Plan>
{
    public void Configure(EntityTypeBuilder<Plan> builder)
    {
        builder.ToTable("Plans", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint(
                "CK_Plans_Price_NonNegative",
                "[Price] >= 0");

            tableBuilder.HasCheckConstraint(
                "CK_Plans_DurationInMonths_Positive",
                "[DurationInMonths] > 0");

            tableBuilder.HasCheckConstraint(
                "CK_Plans_MaxFreezeDays_NonNegative",
                "[MaxFreezeDays] >= 0");

            tableBuilder.HasCheckConstraint(
                "CK_Plans_MaxFreezes_NonNegative",
                "[MaxFreezes] >= 0");

            tableBuilder.HasCheckConstraint(
                "CK_Plans_GuestPassQuota_NonNegative",
                "[GuestPassQuota] >= 0");
        });

        builder.Property(x => x.Id)
            .HasColumnName("PlanId")
            .ValueGeneratedOnAdd();

        builder.HasKey(x => x.Id);

        builder.Property(x => x.PlanName)
            .HasColumnName("PlanName")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Price)
            .HasColumnName("Price")
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.DurationInMonths)
            .HasColumnName("DurationInMonths")
            .IsRequired();

        builder.Property(x => x.MaxFreezeDays)
            .HasColumnName("MaxFreezeDays")
            .IsRequired();

        builder.Property(x => x.MaxFreezes)
            .HasColumnName("MaxFreezes")
            .IsRequired();

        builder.Property(x => x.GuestPassQuota)
            .HasColumnName("GuestPassQuota")
            .IsRequired();

        builder.Property(x => x.AccessScope)
            .HasColumnName("AccessScope")
            .HasConversion(
                accessScope => accessScope.Id,
                id => Enumeration.GetAll<AccessScope>().Single(x => x.Id == id))
            .IsRequired();

        builder.Property(x => x.IsPublished)
            .HasColumnName("IsPublished")
            .IsRequired();
    }
}