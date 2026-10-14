using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TrainBooking.Infrastructure.Entity;

[Table("traintrips")]
[Index("RouteId", Name = "FK_TrainTrips_Route")]
[Index("TrainId", Name = "FK_TrainTrips_Train")]
public partial class Traintrip
{
    [Key]
    public int Id { get; set; }

    public int TrainId { get; set; }

    public int RouteId { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime DepartureTime { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime ArrivalTime { get; set; }

    [Column(TypeName = "enum('Scheduled','Delayed','Completed','Cancelled')")]
    public string Status { get; set; } = null!;

    [Column(TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    [InverseProperty("TrainTrip")]
    public virtual ICollection<Bookingitem> Bookingitems { get; set; } = new List<Bookingitem>();

    [ForeignKey("RouteId")]
    [InverseProperty("Traintrips")]
    public virtual Route Route { get; set; } = null!;

    [ForeignKey("TrainId")]
    [InverseProperty("Traintrips")]
    public virtual Train Train { get; set; } = null!;

    [InverseProperty("TrainTrip")]
    public virtual ICollection<Tripseatprice> Tripseatprices { get; set; } = new List<Tripseatprice>();
}
