using System.Collections.Generic;

namespace furniture_store.Models;

public class HomeViewModel
{
    public List<Category> Categories { get; set; } = new();
    public List<Product> FeaturedProducts { get; set; } = new();
}
