using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TitanFitness.Domain.Common.Entities;
using TitanFitness.Domain.Memberships;

namespace TitanFitness.Infrastructure.Configurations;

public sealed class FreezeConfiguration : IEntityTypeConfiguration<Freeze>
{
    public void Configure(EntityTypeBuilder<Freeze> builder)
    {
        builder.ToTable("Freezes", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint(
                "CK_Freezes_DurationInMonths_Positive",
                "[DurationInMonths] > 0");

            tableBuilder.HasCheckConstraint(
                "CK_Freezes_EndDate_AfterOrEqual_StartDate",
                "[EndDate] >= [StartDate]");
        });

        builder.Property(x => x.Id)
            .HasColumnName("FreezeId")
            .ValueGeneratedOnAdd();

        builder.HasKey(x => x.Id);

        builder.Property(x => x.MembershipId)
            .HasColumnName("MembershipId")
            .IsRequired();

        builder.Property(x => x.StartDate)
            .HasColumnName("StartDate")
            .IsRequired();

        builder.Property(x => x.EndDate)
            .HasColumnName("EndDate")
            .IsRequired();

        builder.Property(x => x.DurationInMonths)
            .HasColumnName("DurationInMonths")
            .IsRequired();

        builder.Property(x => x.Reason)
            .HasColumnName("Reason")
            .HasConversion(
                reason => reason.Id,
                id => Enumeration.GetAll<FreezeReason>().Single(x => x.Id == id))
            .IsRequired();

        builder.Property(x => x.AdditionalNotes)
            .HasColumnName("AdditionalNotes")
            .HasMaxLength(200);

        builder.Property(x => x.RequestedOn)
            .HasColumnName("RequestedOn")
            .IsRequired();

        builder.HasOne<Membership>()
            .WithMany(membership => membership.Freezes)
            .HasForeignKey(x => x.MembershipId)
            .HasConstraintName("FK_Freezes_Memberships_MembershipId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.MembershipId)
            .HasDatabaseName("IX_Freezes_Memberships");
    }
}