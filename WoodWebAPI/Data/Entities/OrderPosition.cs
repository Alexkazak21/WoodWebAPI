namespace WoodWebAPI.Data.Entities;

public partial class OrderPosition
{
    public int Id { get; set; }

    public decimal LengthInMeter { get; set; }

    public decimal DiameterInCantimeter { get; set; }

    public double VolumeInMeter3 { get; set; }
}
