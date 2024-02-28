using Azure.Core.Pipeline;
using System.Runtime.CompilerServices;

namespace WoodWebAPI.Data.Entities;

public class OrderPosition
{
    public int Id { get; set; }
    public required int OrderId { get; set; }
    public decimal DiameterInCantimeter { get; set; }
    public decimal LengthInMeter { get; set; }
    public double VolumeInMeter3 { get; set; }
}
