using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace furniture_store.Models;

[Table("orders")]
public partial class Order
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("user_id")]
    public int? UserId { get; set; }

    [Column("customer_name")]
    [StringLength(100)]
    public string CustomerName { get; set; } = null!;

    [Column("customer_email")]
    [StringLength(150)]
    [Unicode(false)]
    public string? CustomerEmail { get; set; }

    [Column("customer_phone")]
    [StringLength(20)]
    [Unicode(false)]
    public string? CustomerPhone { get; set; }

    [Column("address")]
    public string Address { get; set; } = null!;

    [Column("note")]
    public string? Note { get; set; }

    [Column("total", TypeName = "decimal(15, 0)")]
    public decimal Total { get; set; }

    [Column("status")]
    [StringLength(30)]
    [Unicode(false)]
    public string Status { get; set; } = null!;

    [Column("payment_method")]
    [StringLength(30)]
    [Unicode(false)]
    public string? PaymentMethod { get; set; }

    [Column("created_at", TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    [InverseProperty("Order")]
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    [ForeignKey("UserId")]
    [InverseProperty("Orders")]
    public virtual User? User { get; set; }
}
