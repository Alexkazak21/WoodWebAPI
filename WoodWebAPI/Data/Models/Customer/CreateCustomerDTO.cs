using System.ComponentModel.DataAnnotations;

namespace WoodWebAPI.Data.Models.Customer;

public class CreateCustomerDTO
{
    [Required(ErrorMessage = "TelegramId is nessesery")]
    public long TelegtamId { get; set; }

    [Required(ErrorMessage = "Username is nessesery")]
    public string Username { get; set; }
    public string Name { get; set; }
}
