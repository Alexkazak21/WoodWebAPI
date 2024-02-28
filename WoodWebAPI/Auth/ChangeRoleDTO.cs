using System.ComponentModel.DataAnnotations;

namespace WoodWebAPI.Auth;

public class ChangeRoleDTO
{
    [Required(ErrorMessage = "TelegtamId is required")]
    public required string TelegramId { get; set; }

    [Required(ErrorMessage = "NewRole is required")]
    public required string NewRole { get; set; }
}
