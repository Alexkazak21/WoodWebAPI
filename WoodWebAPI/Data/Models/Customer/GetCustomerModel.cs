namespace WoodWebAPI.Data.Models.Customer;

public class GetCustomerModel
{
    public long TelegramId { get; set; }
    public string? Name { get; set; }
    public string? Username { get; set; }
    public ICollection<Entities.Order> Orders { get; set; } = new List<Entities.Order>();
}
