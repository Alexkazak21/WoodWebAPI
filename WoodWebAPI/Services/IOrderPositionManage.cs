using WoodWebAPI.Data.Models;
using WoodWebAPI.Data.Models.OrderPosition;

namespace WoodWebAPI.Services;

public interface IOrderPositionManage
{
    Task<OrderPositionsModel> GetOrderPositionsOfOrderAsync(GetOrderPositionsByOrderIdDTO model);
    Task<ExecResultModel> AddOrderPositionToOrderAsync(AddOrderPositionDTO model);
    Task<double> GetTotalVolumeOfOrderAsync(GetOrderPositionsByOrderIdDTO model);
    Task<ExecResultModel> UpdateOrderPositionAsync(UpdateOrderPositionDTO model);
}
