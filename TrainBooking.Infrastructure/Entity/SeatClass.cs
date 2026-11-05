using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TrainBooking.Infrastructure.Entity;

[Table("SeatClasses")]
[Index("Code", Name = "Code", IsUnique = true)]
public partial class SeatClass
{
    [Key]
    public int Id { get; set; }

    [StringLength(30)]
    public string Code { get; set; } = null!;

    [StringLength(100)]
    public string Name { get; set; } = null!;

    [InverseProperty("SeatClass")]
    public virtual ICollection<Seat> Seats { get; set; } = new List<Seat>();

    [InverseProperty("SeatClass")]
    public virtual ICollection<Fare> Fares { get; set; } = new List<Fare>();
}
