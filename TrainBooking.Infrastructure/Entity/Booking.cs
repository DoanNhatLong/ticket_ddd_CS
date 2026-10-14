using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TrainBooking.Infrastructure.Entity;

[Table("bookings")]
[Index("BookingCode", Name = "BookingCode", IsUnique = true)]
[Index("UserId", Name = "FK_Bookings_User")]
public partial class Booking
{
    [Key]
    public int Id { get; set; }

    public int UserId { get; set; }

    [StringLength(50)]
    public string BookingCode { get; set; } = null!;

    [Precision(12, 2)]
    public decimal TotalAmount { get; set; }

    [StringLength(50)]
    public string Status { get; set; } = null!;

    [Column(TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime ExpiresAt { get; set; }

    [InverseProperty("Booking")]
    public virtual ICollection<Bookingitem> Bookingitems { get; set; } = new List<Bookingitem>();

    [InverseProperty("Booking")]
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    [ForeignKey("UserId")]
    [InverseProperty("Bookings")]
    public virtual User User { get; set; } = null!;
}
