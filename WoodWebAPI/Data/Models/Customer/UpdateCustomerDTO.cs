using System.ComponentModel.DataAnnotations;

namespace WoodWebAPI.Data.Models.Customer;

public class UpdateCustomerDTO
{
    [Required]
    public int CustomerId { get; set; }
    public string Name { get; set; }
}
