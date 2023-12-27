using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WoodWebAPI.Data.Entities;

public partial class Order
{ 
    public int Id { get; set; }

    public int OrderId { get; set; }

    public int CustomerId { get; set; }  

    public DateTime CreatedAt { get; set; }
    public bool IsVerified { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime CompletedAt { get; set; }
    public virtual ICollection<Timber> Timbers { get; set;} = new List<Timber>();
}
