using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TitanFitness.Domain.Branches;

namespace TitanFitness.Infrastructure.Configurations;

public sealed class StudioConfiguration : IEntityTypeConfiguration<Studio>
{
    public void Configure(EntityTypeBuilder<Studio> builder)
    {
        builder.ToTable("Studios", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint(
                "CK_Studios_Capacity_Positive",
                "[Capacity] > 0");
        });

        builder.Property(x => x.Id)
            .HasColumnName("StudioId")
            .ValueGeneratedOnAdd();

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .HasColumnName("Name")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.BranchId)
            .HasColumnName("BranchId")
            .IsRequired();

        builder.Property(x => x.Capacity)
            .HasColumnName("Capacity")
            .IsRequired();

        builder.HasOne<Branch>()
            .WithMany(branch => branch.Studios)
            .HasForeignKey(x => x.BranchId)
            .HasConstraintName("FK_Studios_Branches_BranchId")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.BranchId)
            .HasDatabaseName("IX_Studios_Branches");

        builder.HasData(
            new
            {
                Id = 1,
                Name = "Studio A",
                BranchId = 1,
                Capacity = 20
            },
            new
            {
                Id = 2,
                Name = "Zen Room",
                BranchId = 1,
                Capacity = 15
            },
            new
            {
                Id = 3,
                Name = "Cycle Studio",
                BranchId = 1,
                Capacity = 25
            },
            new
            {
                Id = 4,
                Name = "Studio A",
                BranchId = 2,
                Capacity = 20
            },
            new
            {
                Id = 5,
                Name = "Strength Studio",
                BranchId = 2,
                Capacity = 18
            },
            new
            {
                Id = 6,
                Name = "Cycle Studio",
                BranchId = 2,
                Capacity = 25
            },
            new
            {
                Id = 7,
                Name = "Yoga Studio",
                BranchId = 3,
                Capacity = 15
            },
            new
            {
                Id = 8,
                Name = "Studio B",
                BranchId = 3,
                Capacity = 20
            },
            new
            {
                Id = 9,
                Name = "Functional Studio",
                BranchId = 3,
                Capacity = 16
            },
            new
            {
                Id = 10,
                Name = "Studio A",
                BranchId = 4,
                Capacity = 20
            },
            new
            {
                Id = 11,
                Name = "Zen Room",
                BranchId = 4,
                Capacity = 15
            },
            new
            {
                Id = 12,
                Name = "Cycle Studio",
                BranchId = 4,
                Capacity = 25
            });
    }
}