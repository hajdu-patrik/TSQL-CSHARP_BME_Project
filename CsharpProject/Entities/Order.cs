using System;
using System.Collections.Generic;

namespace CsharpProject.Entities;

public partial class Order
{
    public int ID { get; set; }
    public DateTime Date { get; set; }
    public DateTime Deadline { get; set; }
    public string Status { get; set; } = null!;
    public string PaymentMethod { get; set; } = null!;
    public int ProductID { get; set; }
    public virtual Product Product { get; set; } = null!;
}
