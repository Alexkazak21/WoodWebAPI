using WoodWebAPI.Data.Models;
using WoodWebAPI.Data.Models.Customer;

namespace WoodWebAPI.Services;

public interface ICustomerManage 
{

    Task<ExecResultModel> CreateAsync(CreateCustomerDTO model);

    Task<ExecResultModel> UpdateAsync(UpdateCustomerDTO model);

    Task<ExecResultModel> DeleteAsync(DeleteCustomerDTO model);

    Task<GetCustomerModel[]?> GetAsync();

    Task<GetCustomerAdmin[]?> GetCustomerByAdminAsync();

}
