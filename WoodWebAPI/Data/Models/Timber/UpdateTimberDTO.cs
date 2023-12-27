using System.ComponentModel.DataAnnotations;

namespace WoodWebAPI.Data.Models.Timber;

public class UpdateTimberDTO
{
    public int OrderId { get; set; }
    public int TimberId { get; set; }
    public double Length { get; set; }
    public int Diameter { get; set; }
}
