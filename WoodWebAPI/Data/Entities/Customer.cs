using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;

namespace WoodWebAPI.Data.Entities;

public partial class Customer
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public long TelegramID { get; set; }
    public string Username { get; set; } = null!;
    public virtual ICollection<Order> Orders { get; set; } = [];
}
