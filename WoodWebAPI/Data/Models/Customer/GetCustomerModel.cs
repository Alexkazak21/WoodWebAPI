namespace WoodWebAPI.Data.Models.Customer;

public class GetCustomerModel
{
    public string? TelegramId { get; set; }
    public string? Name { get; set; }

    public ICollection<Entities.Order> Orders { get; set; } = new List<Entities.Order>();
}
