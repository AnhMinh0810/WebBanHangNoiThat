using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace furniture_store.Models;

[Table("categories")]
[Index("Slug", Name = "UQ__categori__32DD1E4CAC334A1F", IsUnique = true)]
public partial class Category
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    [StringLength(100)]
    public string Name { get; set; } = null!;

    [Column("slug")]
    [StringLength(100)]
    [Unicode(false)]
    public string Slug { get; set; } = null!;

    [Column("image_url")]
    public string? ImageUrl { get; set; }

    [InverseProperty("Category")]
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
