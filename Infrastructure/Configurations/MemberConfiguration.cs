using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TitanFitness.Domain.Branches;
using TitanFitness.Domain.Members;

namespace TitanFitness.Infrastructure.Configurations;

public sealed class MemberConfiguration : IEntityTypeConfiguration<Member>
{
    public void Configure(EntityTypeBuilder<Member> builder)
    {
        builder.ToTable("Members");

        builder.Property(x => x.Id)
            .HasColumnName("MemberId")
            .ValueGeneratedOnAdd();

        builder.HasKey(x => x.Id);

        builder.OwnsOne(x => x.MembershipNumber, membershipNumber =>
        {
            membershipNumber.Property(x => x.Value)
                .HasColumnName("MembershipNumber")
                .HasMaxLength(10)
                .IsRequired();

            membershipNumber.HasIndex(x => x.Value)
                .IsUnique()
                .HasDatabaseName("IX_Members_MembershipNumber");
        });

        builder.Property(x => x.FullName)
            .HasColumnName("FullName")
            .HasMaxLength(100)
            .IsRequired();

        builder.OwnsOne(x => x.Email, email =>
        {
            email.Property(x => x.Value)
                .HasColumnName("Email")
                .HasMaxLength(100);
        });

        builder.OwnsOne(x => x.Phone, phone =>
        {
            phone.Property(x => x.Value)
                .HasColumnName("Phone")
                .HasMaxLength(20);
        });

        builder.OwnsOne(x => x.Address, address =>
        {
            address.Property(x => x.Value)
                .HasColumnName("Address")
                .HasMaxLength(200);
        });

        builder.Property(x => x.JoinedDate)
            .HasColumnName("JoinedDate")
            .IsRequired();

        builder.Property(x => x.Photo)
            .HasColumnName("Photo")
            .HasMaxLength(500);

        builder.Property(x => x.HomeBranchId)
            .HasColumnName("HomeBranchId")
            .IsRequired();

        builder.HasOne<Branch>()
            .WithMany()
            .HasForeignKey(x => x.HomeBranchId)
            .HasConstraintName("FK_Members_Branches_HomeBranchId")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.HomeBranchId)
            .HasDatabaseName("IX_Members_Branches");
    }
}