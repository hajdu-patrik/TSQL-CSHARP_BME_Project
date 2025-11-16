using System;
using System.Collections.Generic;

namespace CsharpProject.Entities;

public partial class Product {
    public int ID { get; set; }
    public string Name { get; set; } = null!;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public int Vatpercentage { get; set; }
    public int CategoryID { get; set; }
    public string Description { get; set; } = null!;
    public virtual Category Category { get; set; } = null!;
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
