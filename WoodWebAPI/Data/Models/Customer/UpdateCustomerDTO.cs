namespace WoodWebAPI.Data.Models.Customer;

public class UpdateCustomerDTO
{
    public required int CustomerId { get; set; }
    public required string Name { get; set; }
}
