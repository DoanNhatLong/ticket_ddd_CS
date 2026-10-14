using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TrainBooking.Infrastructure.Entity;

[Table("seats")]
[Index("CarriageId", Name = "FK_Seats_Carriage")]
public partial class Seat
{
    [Key]
    public int Id { get; set; }

    public int CarriageId { get; set; }

    [StringLength(10)]
    public string SeatNumber { get; set; } = null!;

    [StringLength(50)]
    public string SeatType { get; set; } = null!;

    [InverseProperty("Seat")]
    public virtual ICollection<Bookingitem> Bookingitems { get; set; } = new List<Bookingitem>();

    [ForeignKey("CarriageId")]
    [InverseProperty("Seats")]
    public virtual Carriage Carriage { get; set; } = null!;
}
