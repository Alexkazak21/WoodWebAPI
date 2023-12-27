namespace WoodWebAPI.Data.Models.Timber;

public class GetTimberArray
{
    public int OrderId { get; set; }
    public List<GetTimberArrayDTO> Timbers { get; set; }
}
