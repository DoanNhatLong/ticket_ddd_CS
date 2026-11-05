using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TrainBooking.Infrastructure.Entity;

[Table("Seats")]
[Index("CarriageId", Name = "FK_Seats_Carriage")]
[Index("SeatClassId", Name = "FK_Seats_SeatClass")]
public partial class Seat
{
    [Key]
    public int Id { get; set; }

    public int CarriageId { get; set; }

    [StringLength(10)]
    public string SeatNumber { get; set; } = null!;

    public int SeatClassId { get; set; }

    [InverseProperty("Seat")]
    public virtual ICollection<Bookingitem> Bookingitems { get; set; } = new List<Bookingitem>();

    [ForeignKey("CarriageId")]
    [InverseProperty("Seats")]
    public virtual Carriage Carriage { get; set; } = null!;

    [ForeignKey("SeatClassId")]
    [InverseProperty("Seats")]
    public virtual SeatClass SeatClass { get; set; } = null!;
}
