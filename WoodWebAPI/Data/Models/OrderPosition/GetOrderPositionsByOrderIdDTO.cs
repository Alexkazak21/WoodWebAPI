namespace WoodWebAPI.Data.Models.OrderPosition;

public class GetOrderPositionsByOrderIdDTO
{
    public required long TelegramId {  get; set; }
    public required int OrderId { get; set; }
}
