namespace WoodWebAPI.Data.Models.Timber;

public class GetTimber
{
   public int OrderId { get; set; }
   public List<Entities.Timber>? Timbers { get; set; }
}
