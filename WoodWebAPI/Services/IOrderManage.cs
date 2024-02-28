using WoodWebAPI.Data.Models.Customer;
using WoodWebAPI.Data.Models;
using WoodWebAPI.Data.Models.Order;

namespace WoodWebAPI.Services;

public interface IOrderManage
{
    Task<ExecResultModel> CreateAsync(GetOrdersDTO model);
    Task<ExecResultModel> UpdateAsync();
    Task<ExecResultModel> ArchiveAsync(ArchiveOrderDTO model);
    Task<ExecResultModel> ArchiveByAdminAsync(ArchiveOrderDTO model);
    Task<OrderModel[]?> GetOrdersOfCustomerAsync(GetOrdersDTO model);
    Task<OrderModel[]?> GetFullOrdersArrayAsync();
    Task<ExecResultModel> ChangeStatusOfOrderAsync(ChangeStatusDTO model);
}
