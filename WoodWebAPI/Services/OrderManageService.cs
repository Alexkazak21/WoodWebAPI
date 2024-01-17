using Microsoft.EntityFrameworkCore;
using WoodWebAPI.Data;
using WoodWebAPI.Data.Entities;
using WoodWebAPI.Data.Models;
using WoodWebAPI.Data.Models.Order;

namespace WoodWebAPI.Services
{
    public class OrderManageService : IOrderManage
    {
        private readonly WoodDBContext _db;

        public OrderManageService(WoodDBContext db)
        {
            _db = db;
        }

        public async Task<ExecResultModel> CreateAsync(CreateOrderDTO model)
        {
            var customer = await _db.Customers.Where(x => x.TelegramID == model.Customer_Telegram_Id).FirstOrDefaultAsync();

            if (customer != null)
            {
                int maxOrderNumber = -1;
                try
                {
                    maxOrderNumber = await _db.Orders.Where(x => x.CustomerId == customer.CustomerId).Select(x => x.OrderId).MaxAsync();
                }
                catch (Exception ex)
                {
                    maxOrderNumber = 0;
                }
                if (maxOrderNumber > -1)
                {
                    var order = await _db.Orders.AddAsync(
                        new Data.Entities.Order
                        {
                            CreatedAt = DateTime.Now,
                            CustomerId = customer.CustomerId,
                            IsCompleted = false,
                            IsVerified = false,
                            CompletedAt = DateTime.MinValue,
                            OrderId = maxOrderNumber + 1,
                            Timbers = new List<Data.Entities.Timber>(),
                        }
                        );
                    await _db.SaveChangesAsync();

                    return new ExecResultModel()
                    {
                        Success = true,
                        Message = $"Order {order.Entity.OrderId} was added successfully to Customer {customer.Name}!",
                    };
                }
            }

            return new ExecResultModel()
            {
                Success = false,
                Message = "Can`t find Customer to add any order",
            };
        }

        public async Task<ExecResultModel> Delete(Order[] data)
        {
            if (data.Length == 1)
            {
                _db.Orders.Remove(data[0]);
                await _db.SaveChangesAsync();
            }
            else if (data.Length == 0)
            {
                return new ExecResultModel()
                {
                    Success = false,
                    Message = $"NO orders with specified ID was found ",
                };
            }
            else
            {
                return new ExecResultModel()
                {
                    Success = false,
                    Message = "More then 1 order found",
                };
            }

            return new ExecResultModel()
            {
                Success = true,
                Message = "Order was fully deleted",
            };
        }
        public async Task<ExecResultModel> DeleteAsync(DeleteOrderDTO model)
        {
            if (model != null)
            {
                var customer = await _db.Customers.Where(x => x.TelegramID == model.CustomerTelegramId).FirstOrDefaultAsync();
                if (customer != null)
                {
                    var data = await _db.Orders.Where((x) => x.CustomerId == customer.CustomerId && x.OrderId == model.OrderId && x.IsVerified == false).ToArrayAsync();
                    return await Delete(data);
                }
                return new ExecResultModel()
                {
                    Success = false,
                    Message = "Specified customer not found",
                };
            }
            else
            {
                return new ExecResultModel()
                {
                    Success = false,
                    Message = $"No data presented",
                };
            }
        }

        public async Task<ExecResultModel> DeleteByAdminAsync(DeleteOrderDTO model)
        {
            if (model != null)
            {
                var customer = await _db.Customers.Where(x => x.TelegramID == model.CustomerTelegramId).FirstOrDefaultAsync();

                if (customer != null)
                {
                    var data = await _db.Orders.Where((x) => x.CustomerId == customer.CustomerId && x.OrderId == model.OrderId).ToArrayAsync();
                    return await Delete(data);
                }
                return new ExecResultModel()
                {
                    Success = false,
                    Message = "Specified customer not found",
                };
            }
            else
            {
                return new ExecResultModel()
                {
                    Success = false,
                    Message = $"No data presented",
                };
            }
        }

        public async Task<OrderModel[]?> GetFullOrdersArrayAsync()
        {
            var data = await _db.Orders.Include(x => x.Timbers).ToArrayAsync();

            List<OrderModel> result = new List<OrderModel>();
            if (data != null)
            {
                foreach (var order in data)
                {
                    result.Add(
                        new OrderModel
                        {
                            OrderId = order.OrderId,
                            CustomerId = order.CustomerId,
                            Id = order.Id,
                            CreatedAt = order.CreatedAt,
                            IsCompleted = order.IsCompleted,
                            CompletedAt = order.CompletedAt,
                            IsVerified = order.IsVerified,
                            Timbers = order.Timbers,
                        }
                        );
                }

                return result.ToArray();
            }

            return null;
        }

        public async Task<OrderModel[]?> GetOrdersOfCustomerAsync(GetOrdersDTO model)
        {
            var customer = await _db.Customers.Where(x => x.TelegramID == model.Customer_TelegramID).FirstOrDefaultAsync();
            if (customer != null)
            {
                var data = await _db.Orders.Where(x => x.CustomerId == customer.CustomerId && x.IsCompleted == false).Include(x => x.Timbers).ToArrayAsync();

                List<OrderModel> result = [];
                if (data != null)
                {
                    foreach (var order in data)
                    {
                        result.Add(
                            new OrderModel
                            {
                                OrderId = order.OrderId,
                                CustomerId = order.CustomerId,
                                Id = order.Id,
                                CreatedAt = order.CreatedAt,
                                IsCompleted = order.IsCompleted,
                                CompletedAt = order.CompletedAt,
                                IsVerified = order.IsVerified,
                                Timbers = order.Timbers,
                            });
                    }

                    return result.ToArray();
                }
            }
            else
            {
                return Array.Empty<OrderModel>();
            }


            return Array.Empty<OrderModel>();
        }

        public Task<ExecResultModel> UpdateAsync()
        {
            throw new NotImplementedException();
        }


        public async Task<ExecResultModel> VerifyOrderByAdminAsync(VerifyOrderDTO model)
        {
            if (model != null)
            {
                var customer = await _db.Customers.Where(x => x.TelegramID == model.CustomerTelegramId).FirstOrDefaultAsync();
                
                if (customer != null)
                {
                    var orders = await _db.Orders.Where(x => x.CustomerId == customer.CustomerId && x.IsVerified == false && x.Id == model.OrderId).FirstOrDefaultAsync();

                    if (orders != null) 
                    {
                        orders.IsVerified = true;
                        await _db.SaveChangesAsync();

                        return new ExecResultModel()
                        {
                            Success = true,
                            Message = "Order was verified",
                        };
                    }
                    else
                    {
                        return new ExecResultModel()
                        {
                            Success = false,
                            Message = "selected order doesn`t belong to specified user or already verified",
                        };
                    }
                }
                else
                {
                    return new ExecResultModel()
                    {
                        Success = false,
                        Message = "no user found",
                    };
                }

            }
            else
            {
                return new ExecResultModel()
                {
                    Success = false,
                    Message = "Input model was empty",
                };
            }
        }
    }
}
