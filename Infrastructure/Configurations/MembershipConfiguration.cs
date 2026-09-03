using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TitanFitness.Domain.Common.Entities;
using TitanFitness.Domain.Members;
using TitanFitness.Domain.Memberships;
using TitanFitness.Domain.Plans;

namespace TitanFitness.Infrastructure.Configurations;

public sealed class MembershipConfiguration : IEntityTypeConfiguration<Membership>
{
    public void Configure(EntityTypeBuilder<Membership> builder)
    {
        builder.ToTable(
            "Memberships",
            tableBuilder =>
            {
                tableBuilder.HasCheckConstraint(
                    "CK_Memberships_EndDate_After_StartDate",
                    "[EndDate] >= [StartDate]");
            });

        builder.HasKey(membership => membership.Id);

        builder.Property(membership => membership.Id)
            .HasColumnName("MembershipId")
            .ValueGeneratedOnAdd();

        builder.Property(membership => membership.MemberId)
            .HasColumnName("MemberId")
            .IsRequired();

        builder.Property(membership => membership.PlanId)
            .HasColumnName("PlanId")
            .IsRequired();

        builder.Property(membership => membership.PurchaseDate)
            .HasColumnName("PurchaseDate")
            .IsRequired();

        builder.Property(membership => membership.StartDate)
            .HasColumnName("StartDate")
            .IsRequired();

        builder.Property(membership => membership.EndDate)
            .HasColumnName("EndDate")
            .IsRequired();

        builder.Property(membership => membership.Status)
            .HasColumnName("Status")
            .HasConversion(
                status => status.Id,
                id => Enumeration.GetAll<MembershipStatus>()
                    .Single(status => status.Id == id))
            .IsRequired();

        builder.OwnsOne(
            membership => membership.AgreedTerms,
            agreedTerms =>
            {
                agreedTerms.Property(terms => terms.PricePaid)
                    .HasColumnName("PricePaid")
                    .HasPrecision(18, 2)
                    .IsRequired();

                agreedTerms.Property(terms => terms.DurationInMonths)
                    .HasColumnName("DurationInMonths")
                    .IsRequired();

                agreedTerms.Property(terms => terms.MaxFreezeDays)
                    .HasColumnName("MaxFreezeDays")
                    .IsRequired();

                agreedTerms.Property(terms => terms.MaxFreezes)
                    .HasColumnName("MaxFreezes")
                    .IsRequired();

                agreedTerms.Property(terms => terms.GuestPassQuota)
                    .HasColumnName("GuestPassQuota")
                    .IsRequired();

                agreedTerms.Property(terms => terms.AccessScope)
                    .HasColumnName("AccessScope")
                    .HasConversion(
                        accessScope => accessScope.Id,
                        id => Enumeration.GetAll<AccessScope>()
                            .Single(accessScope => accessScope.Id == id))
                    .IsRequired();
            });

        builder.HasOne<Member>()
            .WithMany()
            .HasForeignKey(membership => membership.MemberId)
            .HasConstraintName("FK_Memberships_Members_MemberId")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Plan>()
            .WithMany()
            .HasForeignKey(membership => membership.PlanId)
            .HasConstraintName("FK_Memberships_Plans_PlanId")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(membership => membership.MemberId)
            .HasDatabaseName("IX_Memberships_Members");

        builder.HasIndex(membership => membership.PlanId)
            .HasDatabaseName("IX_Memberships_Plans");

        builder.Navigation(membership => membership.Freezes)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Navigation(membership => membership.GuestPasses)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}