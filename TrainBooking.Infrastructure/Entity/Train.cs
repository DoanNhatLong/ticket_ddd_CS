using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TrainBooking.Infrastructure.Entity;

[Table("trains")]
[Index("Code", Name = "Code", IsUnique = true)]
public partial class Train
{
    [Key]
    public int Id { get; set; }

    [StringLength(20)]
    public string Code { get; set; } = null!;

    [StringLength(100)]
    public string Name { get; set; } = null!;

    public int TotalCapacity { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    [InverseProperty("Train")]
    public virtual ICollection<Carriage> Carriages { get; set; } = new List<Carriage>();

    [InverseProperty("Train")]
    public virtual ICollection<Traintrip> Traintrips { get; set; } = new List<Traintrip>();
}
