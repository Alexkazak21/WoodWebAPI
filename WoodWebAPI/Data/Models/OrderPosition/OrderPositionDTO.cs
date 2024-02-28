namespace WoodWebAPI.Data.Models.OrderPosition;

public class OrderPositionDTO
{
    public int OrderPositionId { get; set; }
    public decimal LengthInMeter { get; set; }
    public decimal DiameterInCantimeter { get; set; }
    public double VolumeInMeter3 { get; set; }
}
