using Microsoft.Identity.Client;
using WoodWebAPI.Data.Models;
using WoodWebAPI.Data.Models.Timber;

namespace WoodWebAPI.Services;

public interface ITimberManage
{
    Task<GetTimber> GetFullOrderTimbersArrayAsync(GetTimberDTO model);
    Task<ExecResultModel> AddTimberToOrderAsync(AddTimberDTO model);
    Task<ExecResultModel> GetTotalVolumeOfOrderAsync(GetTimberDTO model);
    Task<ExecResultModel> UpdateTimberAsync(UpdateTimberDTO model);
    Task<GetTimberArray> GetTimberArrayAsync(GetTimberByOrderDTO model);

}
