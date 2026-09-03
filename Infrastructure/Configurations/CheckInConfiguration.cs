using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TitanFitness.Domain.Branches;
using TitanFitness.Domain.CheckIns;
using TitanFitness.Domain.Common.Entities;
using TitanFitness.Domain.Members;

namespace TitanFitness.Infrastructure.Configurations;

public sealed class CheckInConfiguration : IEntityTypeConfiguration<CheckIn>
{
    public void Configure(EntityTypeBuilder<CheckIn> builder)
    {
        builder.ToTable("CheckIns");

        builder.Property(x => x.Id)
            .HasColumnName("CheckInId")
            .ValueGeneratedOnAdd();

        builder.HasKey(x => x.Id);

        builder.Property(x => x.MemberId)
            .HasColumnName("MemberId")
            .IsRequired();

        builder.Property(x => x.BranchId)
            .HasColumnName("BranchId")
            .IsRequired();

        builder.Property(x => x.DateTime)
            .HasColumnName("DateTime")
            .IsRequired();

        builder.Property(x => x.CheckInResult)
            .HasColumnName("Result")
            .HasConversion(
                result => result.Id,
                id => Enumeration.GetAll<CheckInResult>().Single(x => x.Id == id))
            .IsRequired();

        builder.Property(x => x.RefusalReason)
            .HasColumnName("RefusalReason")
            .HasMaxLength(100);

        builder.HasOne<Member>()
            .WithMany()
            .HasForeignKey(x => x.MemberId)
            .HasConstraintName("FK_CheckIns_Members_MemberId")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Branch>()
            .WithMany()
            .HasForeignKey(x => x.BranchId)
            .HasConstraintName("FK_CheckIns_Branches_BranchId")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.MemberId)
            .HasDatabaseName("IX_CheckIns_Members");

        builder.HasIndex(x => x.BranchId)
            .HasDatabaseName("IX_CheckIns_Branches");
    }
}