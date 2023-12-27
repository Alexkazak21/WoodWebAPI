using System.ComponentModel.DataAnnotations;

namespace WoodWebAPI.Data.Models.Customer;

public class CreateCustomerDTO
{
    [Required(ErrorMessage = "TelegramId is nessesery ")]
    public string TelegtamId { get; set; }

    public string Name { get; set; }
}
