using System.ComponentModel.DataAnnotations;

namespace WoodWebAPI.Data.Models.Timber;

public class AddTimberDTO
{
    [Required]
    public int OrderId { get; set; }
    [Required]
    public double Length { get; set; }
    [Required]
    public int Diameter { get; set; }
}
