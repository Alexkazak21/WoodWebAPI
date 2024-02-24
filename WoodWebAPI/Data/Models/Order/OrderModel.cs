using WoodWebAPI.Data.Entities;

namespace WoodWebAPI.Data.Models.Order;

public class OrderModel
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public DateTime CreatedAt { get; set; }
    public OrderStatus Status { get; set; }
    public DateTime CompletedAt { get; set; }
    public virtual ICollection<Entities.OrderPosition> OrderPositions { get; set; } = new List<Entities.OrderPosition>();
}
