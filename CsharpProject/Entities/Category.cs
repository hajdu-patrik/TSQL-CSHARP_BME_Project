using System;
using System.Collections.Generic;

namespace CsharpProject.Entities;

public partial class Category
{
    public int ID { get; set; }
    public string Name { get; set; } = null!;
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
