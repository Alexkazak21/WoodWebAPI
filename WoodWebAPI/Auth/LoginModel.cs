using System.ComponentModel.DataAnnotations;

namespace WoodWebAPI.Auth;

public class LoginModel
{
    [Required(ErrorMessage = "TelegtamId is required")]
    public string? TelegramId { get; set; }
}
