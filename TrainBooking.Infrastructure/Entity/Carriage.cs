using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TrainBooking.Infrastructure.Entity;

[Table("carriages")]
[Index("TrainId", Name = "FK_Carriages_Train")]
public partial class Carriage
{
    [Key]
    public int Id { get; set; }

    public int TrainId { get; set; }

    public int CarriageNumber { get; set; }

    [StringLength(50)]
    public string Type { get; set; } = null!;

    public int TotalSeats { get; set; }

    [InverseProperty("Carriage")]
    public virtual ICollection<Seat> Seats { get; set; } = new List<Seat>();

    [ForeignKey("TrainId")]
    [InverseProperty("Carriages")]
    public virtual Train Train { get; set; } = null!;
}
