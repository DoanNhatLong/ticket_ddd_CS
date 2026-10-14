using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TrainBooking.Infrastructure.Entity;

[Table("payments")]
[Index("BookingId", Name = "FK_Payments_Booking")]
[Index("TransactionId", Name = "TransactionId", IsUnique = true)]
public partial class Payment
{
    [Key]
    public int Id { get; set; }

    public int BookingId { get; set; }

    [StringLength(100)]
    public string TransactionId { get; set; } = null!;

    [Precision(12, 2)]
    public decimal Amount { get; set; }

    [StringLength(50)]
    public string PaymentMethod { get; set; } = null!;

    [StringLength(50)]
    public string Status { get; set; } = null!;

    [Column(TypeName = "datetime")]
    public DateTime? PaidAt { get; set; }

    [ForeignKey("BookingId")]
    [InverseProperty("Payments")]
    public virtual Booking Booking { get; set; } = null!;
}
