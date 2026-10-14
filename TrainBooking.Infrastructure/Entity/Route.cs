using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TrainBooking.Infrastructure.Entity;

[Table("routes")]
[Index("ArrivalStationId", Name = "FK_Routes_ArrivalStation")]
[Index("DepartureStationId", Name = "FK_Routes_DepartureStation")]
public partial class Route
{
    [Key]
    public int Id { get; set; }

    public int DepartureStationId { get; set; }

    public int ArrivalStationId { get; set; }

    [Precision(8, 2)]
    public decimal DistanceKm { get; set; }

    [ForeignKey("ArrivalStationId")]
    [InverseProperty("RouteArrivalStations")]
    public virtual Station ArrivalStation { get; set; } = null!;

    [ForeignKey("DepartureStationId")]
    [InverseProperty("RouteDepartureStations")]
    public virtual Station DepartureStation { get; set; } = null!;

    [InverseProperty("Route")]
    public virtual ICollection<Traintrip> Traintrips { get; set; } = new List<Traintrip>();
}
