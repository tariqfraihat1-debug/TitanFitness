using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TitanFitness.Domain.ClassSessions;
using TitanFitness.Domain.Common.Entities;
using TitanFitness.Domain.Members;

namespace TitanFitness.Infrastructure.Configurations;

public sealed class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.ToTable("Bookings", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint(
                "CK_Bookings_WaitlistPosition_Positive",
                "[WaitlistPosition] IS NULL OR [WaitlistPosition] > 0");
        });

        builder.Property(x => x.Id)
            .HasColumnName("BookingId")
            .ValueGeneratedOnAdd();

        builder.HasKey(x => x.Id);

        builder.Property(x => x.SessionId)
            .HasColumnName("SessionId")
            .IsRequired();

        builder.Property(x => x.MemberId)
            .HasColumnName("MemberId")
            .IsRequired();

        builder.Property(x => x.BookedOn)
            .HasColumnName("BookedOn")
            .IsRequired();

        builder.Property(x => x.Status)
            .HasColumnName("Status")
            .HasConversion(
                status => status.Id,
                id => Enumeration.GetAll<BookingStatus>().Single(x => x.Id == id))
            .IsRequired();

        builder.Property(x => x.WaitlistPosition)
            .HasColumnName("WaitlistPosition");

        builder.Property(x => x.TrainerNotes)
            .HasColumnName("TrainerNotes")
            .HasMaxLength(500);

        builder.HasOne<ClassSession>()
            .WithMany(session => session.Bookings)
            .HasForeignKey(x => x.SessionId)
            .HasConstraintName("FK_Bookings_ClassSessions_SessionId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Member>()
            .WithMany()
            .HasForeignKey(x => x.MemberId)
            .HasConstraintName("FK_Bookings_Members_MemberId")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.SessionId)
            .HasDatabaseName("IX_Bookings_ClassSessions");

        builder.HasIndex(x => x.MemberId)
            .HasDatabaseName("IX_Bookings_Members");
    }
}