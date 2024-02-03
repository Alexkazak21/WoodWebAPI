using System.ComponentModel.DataAnnotations;

namespace WoodWebAPI.Auth;

public class RegisterModel
{    
    [Required(ErrorMessage = "TelegtamID is required")]
    public string? TelegramID { get; set; }
}
