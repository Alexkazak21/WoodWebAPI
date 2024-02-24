namespace WoodWebAPI.Data.Models.OrderPosition;

public class OrderPositionsModel
{
    public int OrderId { get; set; }
    public List<OrderPositionDTO>? OrderPositions { get; set; }
}
