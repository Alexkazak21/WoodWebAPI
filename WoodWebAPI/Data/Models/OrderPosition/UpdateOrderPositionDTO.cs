namespace WoodWebAPI.Data.Models.OrderPosition;

public class UpdateOrderPositionDTO
{
    public int OrderId { get; set; }
    public int OrderPositionId { get; set; }
    public decimal LengthInMeter { get; set; }
    public decimal DiameterInCantimeter { get; set; }
}
