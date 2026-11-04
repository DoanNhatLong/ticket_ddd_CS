using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;

namespace TrainBooking.Infrastructure.Entity;

public partial class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Booking> Bookings { get; set; }

    public virtual DbSet<Bookingitem> Bookingitems { get; set; }

    public virtual DbSet<Carriage> Carriages { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<Route> Routes { get; set; }

    public virtual DbSet<Seat> Seats { get; set; }

    public virtual DbSet<Station> Stations { get; set; }

    public virtual DbSet<Train> Trains { get; set; }

    public virtual DbSet<Traintrip> Traintrips { get; set; }

    public virtual DbSet<Tripseatprice> Tripseatprices { get; set; }

    public virtual DbSet<User> Users { get; set; }

    // NOTE: OnConfiguring was removed on purpose. The context is configured through DI
    // (see DependencyInjection.AddInfrastructure). When re-scaffolding, pass --no-onconfiguring.

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_unicode_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.Status).HasDefaultValueSql("'Pending'");

            entity.HasOne(d => d.User).WithMany(p => p.Bookings)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Bookings_User");
        });

        modelBuilder.Entity<Bookingitem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.HasOne(d => d.Booking).WithMany(p => p.Bookingitems).HasConstraintName("FK_BookingItems_Booking");

            entity.HasOne(d => d.Seat).WithMany(p => p.Bookingitems)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BookingItems_Seat");

            entity.HasOne(d => d.TrainTrip).WithMany(p => p.Bookingitems)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BookingItems_TrainTrip");
        });

        modelBuilder.Entity<Carriage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.HasOne(d => d.Train).WithMany(p => p.Carriages).HasConstraintName("FK_Carriages_Train");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Status).HasDefaultValueSql("'Pending'");

            entity.HasOne(d => d.Booking).WithMany(p => p.Payments).HasConstraintName("FK_Payments_Booking");
        });

        modelBuilder.Entity<Route>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.HasOne(d => d.ArrivalStation).WithMany(p => p.RouteArrivalStations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Routes_ArrivalStation");

            entity.HasOne(d => d.DepartureStation).WithMany(p => p.RouteDepartureStations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Routes_DepartureStation");
        });

        modelBuilder.Entity<Seat>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.HasOne(d => d.Carriage).WithMany(p => p.Seats).HasConstraintName("FK_Seats_Carriage");
        });

        modelBuilder.Entity<Station>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        modelBuilder.Entity<Train>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        modelBuilder.Entity<Traintrip>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.Status).HasDefaultValueSql("'Scheduled'");

            entity.HasOne(d => d.Route).WithMany(p => p.Traintrips)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TrainTrips_Route");

            entity.HasOne(d => d.Train).WithMany(p => p.Traintrips).HasConstraintName("FK_TrainTrips_Train");
        });

        modelBuilder.Entity<Tripseatprice>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.HasOne(d => d.TrainTrip).WithMany(p => p.Tripseatprices).HasConstraintName("FK_TripSeatPrices_TrainTrip");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.Role).HasDefaultValueSql("'Customer'");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
