using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TitanFitness.Domain.Branches;

namespace TitanFitness.Infrastructure.Configurations;

public sealed class BranchConfiguration : IEntityTypeConfiguration<Branch>
{
    public void Configure(EntityTypeBuilder<Branch> builder)
    {
        builder.ToTable("Branches", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint(
                "CK_Branches_ClosingTime_After_OpeningTime",
                "[ClosingTime] > [OpeningTime]");
        });

        builder.Property(x => x.Id)
            .HasColumnName("BranchId")
            .ValueGeneratedOnAdd();

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .HasColumnName("Name")
            .HasMaxLength(50)
            .IsRequired();

        builder.OwnsOne(x => x.Address, address =>
        {
            address.Property(x => x.Value)
                .HasColumnName("Address")
                .HasMaxLength(200);
        });

        builder.Property(x => x.OpeningTime)
            .HasColumnName("OpeningTime")
            .IsRequired();

        builder.Property(x => x.ClosingTime)
            .HasColumnName("ClosingTime")
            .IsRequired();

        builder.Navigation(x => x.Studios)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasData(
            new
            {
                Id = 1,
                Name = "Amman Main Branch",
                OpeningTime = new TimeOnly(6, 0),
                ClosingTime = new TimeOnly(23, 0)
            },
            new
            {
                Id = 2,
                Name = "Khalda Branch",
                OpeningTime = new TimeOnly(6, 0),
                ClosingTime = new TimeOnly(23, 0)
            },
            new
            {
                Id = 3,
                Name = "Abdoun Branch",
                OpeningTime = new TimeOnly(7, 0),
                ClosingTime = new TimeOnly(22, 0)
            },
            new
            {
                Id = 4,
                Name = "Sweifieh Branch",
                OpeningTime = new TimeOnly(6, 30),
                ClosingTime = new TimeOnly(22, 30)
            });
    }
}