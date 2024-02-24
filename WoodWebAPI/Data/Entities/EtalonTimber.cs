namespace WoodWebAPI.Data.Entities;

public partial class EtalonTimber
{
    public int Id { get; set; }

    public decimal LengthInMeter { get; set; }

    public decimal DiameterInСantimeter { get; set; }

    public double VolumeInMeter3 { get; set; }
}
