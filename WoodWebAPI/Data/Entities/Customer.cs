using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WoodWebAPI.Data.Entities;

public partial class Customer
{
    public int CustomerId { get; set; }

    public string Name { get; set; } = null!;

    public string TelegramID { get; set; } = null!;

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
