using Microsoft.EntityFrameworkCore;
using WoodWebAPI.Data;
using WoodWebAPI.Data.Entities;
using WoodWebAPI.Data.Models;
using WoodWebAPI.Data.Models.Customer;

namespace WoodWebAPI.Services
{
    public class CustomerManageService : ICustomerManage
    {
        private readonly WoodDBContext _db;

        public CustomerManageService(WoodDBContext dbcontext)
        {
            _db = dbcontext;
        }
        public async Task<ExecResultModel> CreateAsync(CreateCustomerDTO model)
        {
            try
            {
                await _db.Customers.AddAsync(
                new Customer
                {
                    TelegramID = model.TelegtamId,
                    Name = model.Name,
                    Username = model.Username,
                });
                await _db.SaveChangesAsync();
                return new ExecResultModel()
                {
                    Success = true,
                    Message = "Подбзователь успешно добавлен"
                };
            }
            catch (DbUpdateConcurrencyException)
            {
                return new ExecResultModel()
                {
                    Success = false,
                    Message = "Пользователь не добавлен, повторите попытку позже"
                };
            }
            catch (DbUpdateException)
            {
                return new ExecResultModel()
                {
                    Success = false,
                    Message = "Пользователь не добавлен, проверьте входные данные"
                };
            }
        }

        public async Task<ExecResultModel> DeleteAsync(DeleteCustomerDTO model)
        {
            try
            {
                var data = await _db.Customers.Where(x => x.TelegramID == model.TelegramId).FirstAsync();
                _db.Customers.Remove(data);
                await _db.SaveChangesAsync();
                return new ExecResultModel()
                {
                    Success = true,
                    Message = $"Пользователь с TelegramId = {data.TelegramID} был удалён!",
                };
            }
            catch (DbUpdateException)
            {
                return new ExecResultModel()
                {
                    Success = false,
                    Message = $"Пользователя с указанным TelegramId не существует!",
                };
            }
        }


        public Task<GetCustomerAdmin[]?> GetCustomerAsync()
        {
            var customersArray = new List<GetCustomerAdmin>();

            try
            {
                customersArray.AddRange(_db.Customers
                .Select(x => new GetCustomerAdmin
                {
                    TelegramId = x.TelegramID,
                    CustomerId = x.Id,
                    CustomerName = x.Name,
                })
                .DefaultIfEmpty(new()));

                return Task.FromResult<GetCustomerAdmin[]?>([.. customersArray]);
            }
            catch (ArgumentNullException)
            {
                return Task.FromResult<GetCustomerAdmin[]?>([.. customersArray]);
            }
        }
        public Task<GetCustomerModel[]?> GetFullCustomerInfoAsync()
        {
            var customersArray = new List<GetCustomerModel>();

            try
            {
                customersArray.AddRange(_db.Customers
                    .Select(x => new GetCustomerModel
                    {
                        Name = x.Name,
                        TelegramId = x.TelegramID,
                        Username = x.Name,
                        Orders = x.Orders,
                    }));

                return Task.FromResult<GetCustomerModel[]?>([.. customersArray]);
            }
            catch (ArgumentNullException)
            {
                return Task.FromResult<GetCustomerModel[]?>([.. customersArray]);
            }

        }

        // работа с администраторами

        public async Task<ExecResultModel> UpdateAsync(UpdateCustomerDTO model)
        {
            try
            {
                var customer = await _db.Customers
                    .Where(x => x.Id == model.CustomerId)
                    .DefaultIfEmpty(new())
                    .FirstOrDefaultAsync();

                customer.Name = model.Name;

                await _db.SaveChangesAsync();

                return new ExecResultModel
                {
                    Success = true,
                    Message = "Пользователь успешно обновлён",
                };
            }
            catch (DbUpdateException)
            {
                return new ExecResultModel
                {
                    Success = false,
                    Message = "Ошибка при попытке записать данные в БД",
                };
            }
            catch (ArgumentNullException)
            {
                return new ExecResultModel
                {
                    Success = false,
                    Message = "Пользователь не найден",
                };
            }
        }

        public Task<GetAdminDTO[]?> GetAdminListAsync()
        {
            var adminsArray = new List<GetAdminDTO>();

            try
            {
                adminsArray.AddRange(_db.IsAdmin
                .Select(x => new GetAdminDTO
                {
                    AdminRole = x.AdminRole,
                    CreatedAt = x.CreatedAt,
                    Id = x.Id,
                    TelegramId = x.TelegramId,
                    TelegramUsername = x.TelegramUsername,
                })
                .DefaultIfEmpty(new()));

                return Task.FromResult<GetAdminDTO[]?>([.. adminsArray]);
            }
            catch (ArgumentNullException)
            {
                return Task.FromResult<GetAdminDTO[]?>([.. adminsArray]);
            }
        }

        public async Task<ExecResultModel> AddAdminAsync(GetAdminDTO model)
        {
            try
            {
                var customer = await _db.IsAdmin.Where(x => x.TelegramId == model.TelegramId).FirstAsync();

                return new ExecResultModel
                {
                    Success = false,
                    Message = "Администратор уже существует",
                };
            }
            catch (ArgumentNullException)
            {
                await _db.IsAdmin.AddAsync(
                   new IsAdmin
                   {
                       Id = model.Id,
                       CreatedAt = DateTime.Now,
                       AdminRole = model.AdminRole,
                       TelegramId = model.TelegramId,
                       TelegramUsername = model.TelegramUsername,
                   });

                await _db.SaveChangesAsync();

                return new ExecResultModel
                {
                    Success = true,
                    Message = "Администратор успешно добавлен",
                };
            }
            catch (InvalidOperationException invEx)
            {
                return new ExecResultModel
                {
                    Success = false,
                    Message = invEx.Message,
                };
            }
        }
    }
}
