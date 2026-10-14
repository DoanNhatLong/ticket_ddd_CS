using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TrainBooking.Infrastructure.Entity;

[Table("bookingitems")]
[Index("BookingId", Name = "FK_BookingItems_Booking")]
[Index("SeatId", Name = "FK_BookingItems_Seat")]
[Index("TrainTripId", Name = "FK_BookingItems_TrainTrip")]
public partial class Bookingitem
{
    [Key]
    public int Id { get; set; }

    public int BookingId { get; set; }

    public int TrainTripId { get; set; }

    public int SeatId { get; set; }

    [StringLength(100)]
    public string PassengerName { get; set; } = null!;

    [StringLength(20)]
    public string PassengerIdCard { get; set; } = null!;

    [Precision(10, 2)]
    public decimal Price { get; set; }

    [ForeignKey("BookingId")]
    [InverseProperty("Bookingitems")]
    public virtual Booking Booking { get; set; } = null!;

    [ForeignKey("SeatId")]
    [InverseProperty("Bookingitems")]
    public virtual Seat Seat { get; set; } = null!;

    [ForeignKey("TrainTripId")]
    [InverseProperty("Bookingitems")]
    public virtual Traintrip TrainTrip { get; set; } = null!;
}
