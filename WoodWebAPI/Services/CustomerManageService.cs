﻿using Microsoft.EntityFrameworkCore;
using WoodWebAPI.Data;
using WoodWebAPI.Data.Models;
using WoodWebAPI.Data.Entities;
using WoodWebAPI.Data.Models.Customer;
using Microsoft.AspNetCore.Mvc;

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
            await _db.Customers.AddAsync(
                new Customer
                {
                    TelegramID = model.TelegtamId,
                    Name = model.Name,
                    Username = model.Username,
                });

            if(_db.SaveChangesAsync().Result > 0) 
            {
                return new ExecResultModel() 
                {
                    Success = true,
                    Message = "Customer added successfully"
                }; 
            }
            else
            {
                return new ExecResultModel()
                {
                    Success = false,
                    Message = "Customer not added, check the input"
                };
            }
        }

        public async Task<ExecResultModel> DeleteAsync(DeleteCustomerDTO model)
        {
            var data = _db.Customers.Where(x => x.TelegramID == model.TelegramId).First();

            if( data != null ) 
            {
               _db.Customers.Remove(data);
                await _db.SaveChangesAsync();
                return new ExecResultModel()
                {
                    Success = true,
                    Message = $"Customer with TelegramId = {data.TelegramID} was remowed!",
                };
            }
            else
            {
                return new ExecResultModel()
                {
                    Success = false,
                    Message = $"Customer with TelegramId = {data.TelegramID} doesn`t exist!",
                };
            }

        }


        public async Task<GetCustomerAdmin[]> GetCustomerByAdminAsync()
        {
            var customersArray = new List<GetCustomerAdmin>();

            var data = await _db.Customers.CountAsync();

            if (data == 0)
            {
                return null;
            }

            var result = await _db.Customers.ToArrayAsync();

            foreach (var item in result)
            {
                customersArray.Add(new GetCustomerAdmin()
                {
                    CustomerId = item.CustomerId,
                    CustomerName = item.Name,
                    ChatID = item.TelegramID,
                });
            }

            return customersArray.ToArray();
        }
        public async Task<GetCustomerModel[]?> GetAsync()
        {
            var customersArray = new List<GetCustomerModel>();

            var data = await _db.Customers.CountAsync();

            if (data == 0)
            {
                return null;
            }

            var result = await _db.Customers.Include(x => x.Orders).ToArrayAsync();

            foreach (var item in result) 
            {
                customersArray.Add(new GetCustomerModel()
                {
                    TelegramId = item.TelegramID,
                    Name = item.Name,
                    Orders = item.Orders,
                    Username = item.Username,                    
                });
            }

            return customersArray.ToArray();
        }

        public async Task<GetAdminDTO[]?> GetAdminListAsync()
        {
            var adminsArray = new List<GetAdminDTO>();

            var data = await _db.IsAdmin.CountAsync();

            if (data == 0)
            {
                return null;
            }

            var result = await _db.IsAdmin.ToArrayAsync();

            foreach (var item in result)
            {
                adminsArray.Add(new GetAdminDTO()
                     {
                        TelegramId = item.TelegramId,
                        AdminRole = item.AdminRole,
                        CreatedAt = item.CreatedAt,
                        Id = item.Id,
                        TelegramUsername = item.TelegramUsername
                     });
            }

            return adminsArray.ToArray();
        }

        public async Task<ExecResultModel> UpdateAsync(UpdateCustomerDTO model)
        {
            try
            {
                var customer = await _db.Customers.Where(x => x.CustomerId == model.CustomerId).FirstOrDefaultAsync();

                if(customer != null)
                {
                    customer.Name = model.Name;

                    await _db.SaveChangesAsync();

                    return new ExecResultModel
                    {
                        Success = true,
                        Message = "Custoner was updated",
                    };
                }

                return new ExecResultModel
                {
                    Success = false,
                    Message = "Custoner doesn`t exist",
                };

            }
            catch (Exception ex) 
            {
                return new ExecResultModel
                {
                    Success = false,
                    Message = ex.Message,
                };
            }
        }

        public async Task<ExecResultModel> AddAdminAsync(GetAdminDTO model)
        {
            try
            {
                var customer = await _db.IsAdmin.Where(x => x.TelegramId == model.TelegramId).FirstOrDefaultAsync();

                if (customer != null)
                {
                    return new ExecResultModel
                    {
                        Success = false,
                        Message = "Администратор уже существует",
                    };                   
                }

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
            catch (Exception ex)
            {
                return new ExecResultModel
                {
                    Success = false,
                    Message = ex.Message,
                };
            }
        }
    }
}
