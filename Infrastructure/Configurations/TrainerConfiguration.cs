using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TitanFitness.Domain.Branches;
using TitanFitness.Domain.Trainers;

namespace TitanFitness.Infrastructure.Configurations;

public sealed class TrainerConfiguration : IEntityTypeConfiguration<Trainer>
{
    public void Configure(EntityTypeBuilder<Trainer> builder)
    {
        builder.ToTable("Trainers");

        builder.Property(x => x.Id)
            .HasColumnName("TrainerId")
            .ValueGeneratedOnAdd();

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .HasColumnName("Name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.BranchId)
            .HasColumnName("BranchId")
            .IsRequired();

        builder.Property(x => x.Specialty)
            .HasColumnName("Specialty")
            .HasMaxLength(100);

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

        builder.Property(x => x.IsActive)
            .HasColumnName("IsActive")
            .IsRequired();

        builder.HasOne<Branch>()
            .WithMany()
            .HasForeignKey(x => x.BranchId)
            .HasConstraintName("FK_Trainers_Branches_BranchId")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.BranchId)
            .HasDatabaseName("IX_Trainers_Branches");
    }
}