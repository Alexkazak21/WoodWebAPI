using System.ComponentModel.DataAnnotations;

namespace WoodWebAPI.Auth;

public class RegisterModel
{    
    [Required(ErrorMessage = "TelegtamID is required")]
    public string? TelegramID { get; set; }

    [Required(ErrorMessage = "Password is required")]
    public string? Password { get; set; }
}
