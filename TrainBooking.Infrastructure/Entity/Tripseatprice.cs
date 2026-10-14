using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TrainBooking.Infrastructure.Entity;

[Table("tripseatprices")]
[Index("TrainTripId", Name = "FK_TripSeatPrices_TrainTrip")]
public partial class Tripseatprice
{
    [Key]
    public int Id { get; set; }

    public int TrainTripId { get; set; }

    [StringLength(50)]
    public string SeatTypeId { get; set; } = null!;

    [Precision(10, 2)]
    public decimal Price { get; set; }

    [ForeignKey("TrainTripId")]
    [InverseProperty("Tripseatprices")]
    public virtual Traintrip TrainTrip { get; set; } = null!;
}
