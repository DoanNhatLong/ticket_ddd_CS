using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TrainBooking.Infrastructure.Entity;

[Table("stations")]
[Index("Code", Name = "Code", IsUnique = true)]
public partial class Station
{
    [Key]
    public int Id { get; set; }

    [StringLength(10)]
    public string Code { get; set; } = null!;

    [StringLength(100)]
    public string Name { get; set; } = null!;

    [StringLength(100)]
    public string City { get; set; } = null!;

    [Column(TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    [InverseProperty("ArrivalStation")]
    public virtual ICollection<Route> RouteArrivalStations { get; set; } = new List<Route>();

    [InverseProperty("DepartureStation")]
    public virtual ICollection<Route> RouteDepartureStations { get; set; } = new List<Route>();
}
