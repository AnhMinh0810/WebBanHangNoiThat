using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace furniture_store.Models;

[Table("products")]
[Index("Slug", Name = "UQ__products__32DD1E4C71DB409A", IsUnique = true)]
public partial class Product
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    [StringLength(200)]
    public string Name { get; set; } = null!;

    [Column("slug")]
    [StringLength(200)]
    [Unicode(false)]
    public string Slug { get; set; } = null!;

    [Column("description")]
    public string? Description { get; set; }

    [Column("price", TypeName = "decimal(15, 0)")]
    public decimal Price { get; set; }

    [Column("image_url")]
    public string? ImageUrl { get; set; }

    [Column("stock")]
    public int Stock { get; set; }

    [Column("category_id")]
    public int? CategoryId { get; set; }

    [Column("featured")]
    public bool? Featured { get; set; }

    [Column("created_at", TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    [ForeignKey("CategoryId")]
    [InverseProperty("Products")]
    public virtual Category? Category { get; set; }

    [InverseProperty("Product")]
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
