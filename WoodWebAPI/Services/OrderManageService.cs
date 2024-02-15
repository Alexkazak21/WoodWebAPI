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
                        new Order
                        {
                            CreatedAt = DateTime.Now,
                            CustomerId = customer.CustomerId,
                            IsCompleted = false,
                            IsVerified = false,
                            IsPaid = false,
                            CompletedAt = DateTime.MinValue,
                            OrderId = maxOrderNumber + 1,
                            Timbers = new List<Timber>(),
                        }
                        );
                    await _db.SaveChangesAsync();

                    return new ExecResultModel()
                    {
                        Success = true,
                        Message = $"Заказ {order.Entity.Id} был успешно добавлен пользователю {customer.Name}!",
                    };
                }
            }

            return new ExecResultModel()
            {
                Success = false,
                Message = "Не возможно найти пользователя для добавления заказа",
            };
        }

        public async Task<ExecResultModel> Delete(Order[] data)
        {
            if (data.Length == 1)
            {
                
                var context = await _db.Orders.Include(x => x.Timbers).FirstAsync(x => x.Id == data[0].Id);
                _db.Orders.Remove(context);
                await _db.SaveChangesAsync();
            }
            else if (data.Length == 0)
            {
                return new ExecResultModel()
                {
                    Success = false,
                    Message = $"Не найдено заказов с указанным ID",
                };
            }
            else
            {
                return new ExecResultModel()
                {
                    Success = false,
                    Message = "Найдено долее 1 заказа",
                };
            }

            return new ExecResultModel()
            {
                Success = true,
                Message = "Заказ удалён",
            };
        }
        public async Task<ExecResultModel> DeleteAsync(DeleteOrderDTO model)
        {
            if (model != null)
            {
                var customer = await _db.Customers.Where(x => x.TelegramID == model.CustomerTelegramId).FirstOrDefaultAsync();
                if (customer != null)
                {
                    var data = await _db.Orders.Where(x => x.CustomerId == customer.CustomerId && x.Id == model.OrderId && x.IsVerified == false).ToArrayAsync();
                    return await Delete(data);
                }
                return new ExecResultModel()
                {
                    Success = false,
                    Message = "Указанный пользователь не найден",
                };
            }
            else
            {
                return new ExecResultModel()
                {
                    Success = false,
                    Message = $"Данных нет",
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
                    Message = "Указанный пользователь не найден",
                };
            }
            else
            {
                return new ExecResultModel()
                {
                    Success = false,
                    Message = $"Данных нет",
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
                            IsPaid = order.IsPaid,
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
                var data = await _db.Orders.Where(x => x.CustomerId == customer.CustomerId && x.IsPaid == false).Include(x => x.Timbers).ToArrayAsync();

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
                                IsPaid = order.IsPaid,
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
                    var orders = await _db.Orders.Where(x => x.IsVerified == false && x.Id == model.OrderId).FirstOrDefaultAsync();

                    if (orders != null) 
                    {
                        orders.IsVerified = true;
                        await _db.SaveChangesAsync();

                        return new ExecResultModel()
                        {
                            Success = true,
                            Message = "Заказ принят в работу",
                        };
                    }
                    else
                    {
                        return new ExecResultModel()
                        {
                            Success = false,
                            Message = "Выбранный заказ не пренадлежит указанному пользователю или уже полтверждён",
                        };
                    }
            }
            else
            {
                return new ExecResultModel()
                {
                    Success = false,
                    Message = "Входная модель оказалась пуста",
                };
            }
        }

        public async Task<ExecResultModel> CompleteOrderByAdminAsync(VerifyOrderDTO model)
        {
            if (model != null)
            {
                var orders = await _db.Orders.Where(x => x.IsVerified == true && x.Id == model.OrderId).FirstOrDefaultAsync();

                if (orders != null)
                {
                    orders.IsCompleted = true;
                    await _db.SaveChangesAsync();

                    return new ExecResultModel()
                    {
                        Success = true,
                        Message = "Заказ завершён",
                    };
                }
                else
                {
                    return new ExecResultModel()
                    {
                        Success = false,
                        Message = "Выбранный заказ не пренадлежит указанному пользователю или уже полтверждён",
                    };
                }
            }
            else
            {
                return new ExecResultModel()
                {
                    Success = false,
                    Message = "Входная модель оказалась пуста",
                };
            }
        }
    }
}
