using System.ComponentModel.DataAnnotations;

namespace WoodWebAPI.Auth;

public class LoginModel
{
    [Required(ErrorMessage = "TelegtamId is required")]
    public string? TelegramId { get; set; }

    [Required(ErrorMessage = "Password is required")]
    public string? Password { get; set; }
}
