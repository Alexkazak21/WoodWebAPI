using WoodWebAPI.Data.Models.Customer;
using WoodWebAPI.Data.Models;
using WoodWebAPI.Data.Models.Order;

namespace WoodWebAPI.Services;

public interface IOrderManage
{
    Task<ExecResultModel> CreateAsync(CreateOrderDTO model);

    Task<ExecResultModel> UpdateAsync();

    Task<ExecResultModel> DeleteAsync(DeleteOrderDTO model);
    Task<ExecResultModel> DeleteByAdminAsync(DeleteOrderDTO model);

    Task<OrderModel[]?> GetOrdersOfCustomerAsync(GetOrdersDTO model);

    Task<OrderModel[]?> GetFullOrdersArrayAsync();

    Task<ExecResultModel> VerifyOrderByAdminAsync(VerifyOrderDTO model);


}
