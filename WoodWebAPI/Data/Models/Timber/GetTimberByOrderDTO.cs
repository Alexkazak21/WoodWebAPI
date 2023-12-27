using System.ComponentModel.DataAnnotations;

namespace WoodWebAPI.Data.Models.Timber;

public class GetTimberByOrderDTO
{
    [Required]
    public int OrderId { get; set; }
}
