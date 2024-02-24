using System.ComponentModel.DataAnnotations;

namespace WoodWebAPI.Data.Entities;

public partial class Order
{ 
    public int Id { get; set; }
    public long CustomerTelegramId { get; set; }
    public DateTime CreatedAt { get; set; }
    public OrderStatus Status { get;set; }
    public DateTime CompletedAt { get; set; }
    public virtual ICollection<OrderPosition> OrderPositions { get; set;} = [];
}

public enum OrderStatus
{
    NewOrder,
    Canceled,
    Verivied,
    CanceledByAdmin,
    Completed,
    Paid,
    Archived
}

//public void OrderStatusChange(OrderStatus oldStatus, OrderStatus[] newStatus)
//{
//}