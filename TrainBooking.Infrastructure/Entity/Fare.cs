using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TrainBooking.Infrastructure.Entity;

[Table("Fares")]
[Index("TrainTripId", "SeatClassId", Name = "UQ_Fares_TrainTrip_SeatClass", IsUnique = true)]
[Index("SeatClassId", Name = "FK_Fares_SeatClass")]
public partial class Fare
{
    [Key]
    public int Id { get; set; }

    public int TrainTripId { get; set; }

    public int SeatClassId { get; set; }

    [Precision(10, 2)]
    public decimal Price { get; set; }

    [ForeignKey("TrainTripId")]
    [InverseProperty("Fares")]
    public virtual Traintrip TrainTrip { get; set; } = null!;

    [ForeignKey("SeatClassId")]
    [InverseProperty("Fares")]
    public virtual SeatClass SeatClass { get; set; } = null!;
}
