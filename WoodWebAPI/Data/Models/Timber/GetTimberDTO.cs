using System.ComponentModel.DataAnnotations;

namespace WoodWebAPI.Data.Models.Timber;

public class GetTimberDTO
{
    [Required]
    public string? customerTelegramId { get; set; }
    public int OrderId { get; set; }
}
