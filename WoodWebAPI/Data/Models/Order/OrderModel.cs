using WoodWebAPI.Data.Entities;

namespace WoodWebAPI.Data.Models.Order;

public class OrderModel
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public int CustomerId { get; set; }

    public DateTime CreatedAt { get; set; }
    public bool IsVerified { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime CompletedAt { get; set; }
    public virtual ICollection<Entities.Timber> Timbers { get; set; } = new List<Entities.Timber>();
}
