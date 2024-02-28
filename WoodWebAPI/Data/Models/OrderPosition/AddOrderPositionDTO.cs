namespace WoodWebAPI.Data.Models.OrderPosition;

public class AddOrderPositionDTO
{
    public required int OrderId { get; set; }
    public required decimal Length { get; set; }
    public required decimal Diameter { get; set; }
}
