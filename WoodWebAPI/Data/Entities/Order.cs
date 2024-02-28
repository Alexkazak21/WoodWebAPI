using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WoodWebAPI.Data.Entities;

public partial class Order
{ 
    public int Id { get; set; }

    [ForeignKey("CustomerTelegramId")]
    public long CustomerTelegramId { get; set; }
    public Customer? Customer { get; set; }
    public DateTime CreatedAt { get; set; }
    public OrderStatus Status { get;set; }
    public DateTime CompletedAt { get; set; }
    public virtual ICollection<OrderPosition> OrderPositions { get; set;} = [];
}

public enum OrderStatus
{
    NewOrder,
    Approved,
    Verivied,
    CanceledByAdmin,
    Completed,
    Paid,
    Archived
}