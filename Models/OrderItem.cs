using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace furniture_store.Models;

[Table("order_items")]
public partial class OrderItem
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("order_id")]
    public int? OrderId { get; set; }

    [Column("product_id")]
    public int? ProductId { get; set; }

    [Column("product_name")]
    [StringLength(200)]
    public string? ProductName { get; set; }

    [Column("quantity")]
    public int Quantity { get; set; }

    [Column("price", TypeName = "decimal(15, 0)")]
    public decimal Price { get; set; }

    [ForeignKey("OrderId")]
    [InverseProperty("OrderItems")]
    public virtual Order? Order { get; set; }

    [ForeignKey("ProductId")]
    [InverseProperty("OrderItems")]
    public virtual Product? Product { get; set; }
}
