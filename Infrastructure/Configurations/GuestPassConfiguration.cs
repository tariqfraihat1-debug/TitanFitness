using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TitanFitness.Domain.Memberships;

namespace TitanFitness.Infrastructure.Configurations;

public sealed class GuestPassConfiguration : IEntityTypeConfiguration<GuestPass>
{
    public void Configure(EntityTypeBuilder<GuestPass> builder)
    {
        builder.ToTable("GuestPasses", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint(
                "CK_GuestPasses_UsedOn_AfterOrEqual_IssuedOn",
                "[UsedOn] IS NULL OR [UsedOn] >= [IssuedOn]");
        });

        builder.Property(x => x.Id)
            .HasColumnName("GuestPassId")
            .ValueGeneratedOnAdd();

        builder.HasKey(x => x.Id);

        builder.Property(x => x.MembershipId)
            .HasColumnName("MembershipId")
            .IsRequired();

        builder.Property(x => x.IssuedOn)
            .HasColumnName("IssuedOn")
            .IsRequired();

        builder.Property(x => x.UsedOn)
            .HasColumnName("UsedOn");

        builder.Property(x => x.GuestName)
            .HasColumnName("GuestName")
            .HasMaxLength(100);

        builder.HasOne<Membership>()
            .WithMany(membership => membership.GuestPasses)
            .HasForeignKey(x => x.MembershipId)
            .HasConstraintName("FK_GuestPasses_Memberships_MembershipId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.MembershipId)
            .HasDatabaseName("IX_GuestPasses_Memberships");
    }
}