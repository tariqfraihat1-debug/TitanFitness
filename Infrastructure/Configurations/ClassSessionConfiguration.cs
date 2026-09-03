using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TitanFitness.Domain.Branches;
using TitanFitness.Domain.ClassSessions;
using TitanFitness.Domain.Common.Entities;
using TitanFitness.Domain.Trainers;

namespace TitanFitness.Infrastructure.Configurations;

public sealed class ClassSessionConfiguration : IEntityTypeConfiguration<ClassSession>
{
    public void Configure(EntityTypeBuilder<ClassSession> builder)
    {
        builder.ToTable("ClassSessions", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint(
                "CK_ClassSessions_DurationMinutes_Allowed",
                "[DurationMinutes] IN (30, 45, 60)");

            tableBuilder.HasCheckConstraint(
                "CK_ClassSessions_CapacityLimit_Positive",
                "[CapacityLimit] > 0");
        });

        builder.Property(x => x.Id)
            .HasColumnName("SessionId")
            .ValueGeneratedOnAdd();

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ClassName)
            .HasColumnName("ClassName")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.BranchId)
            .HasColumnName("BranchId")
            .IsRequired();

        builder.Property(x => x.StudioId)
            .HasColumnName("StudioId")
            .IsRequired();

        builder.Property(x => x.TrainerId)
            .HasColumnName("TrainerId")
            .IsRequired();

        builder.Property(x => x.SessionDate)
            .HasColumnName("SessionDate")
            .IsRequired();

        builder.Property(x => x.StartTime)
            .HasColumnName("StartTime")
            .IsRequired();

        builder.Property(x => x.DurationMinutes)
            .HasColumnName("DurationMinutes")
            .IsRequired();

        builder.Property(x => x.CapacityLimit)
            .HasColumnName("CapacityLimit")
            .IsRequired();

        builder.Property(x => x.Status)
            .HasColumnName("Status")
            .HasConversion(
                status => status.Id,
                id => Enumeration.GetAll<ClassSessionStatus>().Single(x => x.Id == id))
            .IsRequired();

        builder.Property(x => x.Description)
            .HasColumnName("Description")
            .HasMaxLength(500);

        builder.HasOne<Branch>()
            .WithMany()
            .HasForeignKey(x => x.BranchId)
            .HasConstraintName("FK_ClassSessions_Branches_BranchId")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Studio>()
            .WithMany()
            .HasForeignKey(x => x.StudioId)
            .HasConstraintName("FK_ClassSessions_Studios_StudioId")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Trainer>()
            .WithMany()
            .HasForeignKey(x => x.TrainerId)
            .HasConstraintName("FK_ClassSessions_Trainers_TrainerId")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.BranchId)
            .HasDatabaseName("IX_ClassSessions_Branches");

        builder.HasIndex(x => x.StudioId)
            .HasDatabaseName("IX_ClassSessions_Studios");

        builder.HasIndex(x => x.TrainerId)
            .HasDatabaseName("IX_ClassSessions_Trainers");

        builder.Navigation(x => x.Bookings)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}