using System.ComponentModel.DataAnnotations;

namespace WoodWebAPI.Data.Models.Customer;

public class GetCustomerAdmin
{
    public int CustonerId { get; set; }
    public string CustomerName { get; set; }
    public string ChatID { get; set; }
}
