using WoodWebAPI.Data.Entities;

namespace WoodWebAPI.Data.Models.Order;

public class ChangeStatusDTO
{
    public int OrderId { get; set; }
    public OrderStatus NewStatus { get; set; }
}
