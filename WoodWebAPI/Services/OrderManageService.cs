using Microsoft.EntityFrameworkCore;
using WoodWebAPI.Data;
using WoodWebAPI.Data.Entities;
using WoodWebAPI.Data.Models;
using WoodWebAPI.Data.Models.Order;
using WoodWebAPI.Worker;

namespace WoodWebAPI.Services
{
    public class OrderManageService : IOrderManage
    {
        private readonly WoodDBContext _db;

        public OrderManageService(WoodDBContext db)
        {
            _db = db;
        }

        public async Task<ExecResultModel> CreateAsync(GetOrdersDTO model)
        {
            var customer = await _db.Customers.Where(x => x.TelegramID == model.CustomerTelegramId).FirstOrDefaultAsync();

            if (customer == null)
            {
                return new ExecResultModel()
                {
                    Success = false,
                    Message = "Не возможно найти пользователя для добавления заказа",
                };
            }

            try
            {
                var order = await _db.Orders.AddAsync(
                new Order
                {
                    CreatedAt = DateTime.UtcNow,
                    CustomerTelegramId = model.CustomerTelegramId,
                    Status = OrderStatus.NewOrder,
                    CompletedAt = DateTime.MinValue,
                    OrderPositions = [],
                }
                );
                await _db.SaveChangesAsync();

                return new ExecResultModel()
                {
                    Success = true,
                    Message = $"Заказ {order.Entity.Id} был успешно добавлен пользователю {customer.Name}!",
                };
            }
            catch (DbUpdateException)
            {
                return new ExecResultModel()
                {
                    Success = false,
                    Message = $"Произошла непредвиденная ошибка, попробуйте позже",
                };
            }
        }

        public async Task<ExecResultModel> Archive(Order data)
        {
            try
            {
                var archived = await _db.Orders.Where(x => x.CustomerTelegramId == data.CustomerTelegramId && x.Id == data.Id).FirstAsync();
            }
            catch (ArgumentNullException)
            {
                return new ExecResultModel()
                {
                    Success = false,
                    Message = "Заказ не найден",
                };
            }

            return new ExecResultModel()
            {
                Success = true,
                Message = "Заказ удалён",
            };
        }
        public async Task<ExecResultModel> ArchiveAsync(ArchiveOrderDTO model)
        {
            if (model == null)
            {
                return new ExecResultModel()
                {
                    Success = false,
                    Message = $"Данных нет",
                };
            }
            else
            {
                try
                {
                    var data = await _db.Orders.Where(x => x.CustomerTelegramId == model.CustomerTelegramId && x.Id == model.OrderId && x.Status == OrderStatus.Verivied).FirstAsync();
                    return await Archive(data);
                }
                catch (ArgumentNullException)
                {
                    return new ExecResultModel()
                    {
                        Success = false,
                        Message = "Указанный заказ не найден",
                    };
                }
            }
        }

        public async Task<ExecResultModel> ArchiveByAdminAsync(ArchiveOrderDTO model)
        {
            if (model == null)
            {
                return new ExecResultModel()
                {
                    Success = false,
                    Message = $"Данных нет",
                };
            }
            else
            {
                var data = await _db.Orders.Where(x => x.CustomerTelegramId == model.CustomerTelegramId && x.Id == model.OrderId).FirstAsync();
                return await Archive(data);
            }
        }

        public async Task<OrderModel[]?> GetFullOrdersArrayAsync()
        {
            try
            {
                var ordersArray = await _db.Orders
                .Include(x => x.OrderPositions)
                .Select(x => new OrderModel
                {
                    OrderPositions = x.OrderPositions,
                    CompletedAt = x.CompletedAt,
                    CreatedAt = x.CreatedAt,
                    CustomerId = x.CustomerTelegramId,
                    Status = x.Status,
                    Id = x.Id,
                })
                .ToArrayAsync();

                return ordersArray;
            }
            catch (ArgumentNullException)
            {
                return [];
            }
        }

        public async Task<OrderModel[]?> GetOrdersOfCustomerAsync(GetOrdersDTO model)
        {
            try
            {
                var ordersArray = await _db.Orders
                .Where(x => x.CustomerTelegramId == model.CustomerTelegramId)
                .Include(x => x.OrderPositions)
                .Select(x => new OrderModel
                {
                    CompletedAt = x.CompletedAt,
                    CreatedAt = x.CreatedAt,
                    CustomerId = x.CustomerTelegramId,
                    Status = x.Status,
                    Id = x.Id,
                    OrderPositions = x.OrderPositions

                })
                .ToArrayAsync();

                return ordersArray;
            }
            catch (ArgumentNullException)
            {
                return [];
            }
        }

        public Task<ExecResultModel> UpdateAsync()
        {
            throw new NotImplementedException();
        }


        public async Task<ExecResultModel> VerifyOrderByAdminAsync(VerifyOrderDTO model)
        {
            try
            {
                var order = await _db.Orders.Where(x => x.Status == OrderStatus.NewOrder && x.Id == model.OrderId).FirstAsync();
                var isValid = ConsumedMethods.IsValidTransition(order.Status, OrderStatus.Verivied);
                if (isValid) 
                {
                    order.Status = OrderStatus.Verivied;
                    await _db.SaveChangesAsync();

                    return new ExecResultModel()
                    {
                        Success = true,
                        Message = "Заказ принят в работу",
                    };
                }

                return new ExecResultModel()
                {
                    Success = false,
                    Message = $"Невозможно стенить статус заказа на {OrderStatus.Verivied}",
                };
            }
            catch (ArgumentNullException)
            {
                return new ExecResultModel
                {
                    Success = false,
                    Message = "Данные не найдены"
                };
            }
            catch (DbUpdateException)
            {
                return new ExecResultModel
                {
                    Success = false,
                    Message = "БД занята, попробуйте позже"
                };
            }
        }

        public async Task<ExecResultModel> CompleteOrderByAdminAsync(VerifyOrderDTO model)
        {
            throw new NotFiniteNumberException();
            //if (model != null)
            //{
            //    var orders = await _db.Orders.Where(x => x.IsVerified == true && x.Id == model.OrderId).FirstOrDefaultAsync();

            //    if (orders != null)
            //    {
            //        orders.IsCompleted = true;
            //        await _db.SaveChangesAsync();

            //        return new ExecResultModel()
            //        {
            //            Success = true,
            //            Message = "Заказ завершён",
            //        };
            //    }
            //    else
            //    {
            //        return new ExecResultModel()
            //        {
            //            Success = false,
            //            Message = "Выбранный заказ не пренадлежит указанному пользователю или уже подтверждён",
            //        };
            //    }
            //}
            //else
            //{
            //    return new ExecResultModel()
            //    {
            //        Success = false,
            //        Message = "Входная модель оказалась пуста",
            //    };
            //}
        }

        public async Task<ExecResultModel> PaidSuccessfullyAsync(VerifyOrderDTO model)
        {
            throw new NotImplementedException();
            //if (model != null)
            //{
            //    var orders = await _db.Orders.Where(x => x.IsVerified == true && x.IsCompleted == true && x.IsPaid == false && x.Id == model.OrderId).FirstOrDefaultAsync();

            //    if (orders != null)
            //    {
            //        orders.IsPaid = true;
            //        await _db.SaveChangesAsync();

            //        return new ExecResultModel()
            //        {
            //            Success = true,
            //            Message = "Заказ закрыт",
            //        };
            //    }
            //    else
            //    {
            //        return new ExecResultModel()
            //        {
            //            Success = false,
            //            Message = "Выбранный заказ не пренадлежит указанному пользователю или уже подтверждён",
            //        };
            //    }
            //}
            //else
            //{
            //    return new ExecResultModel()
            //    {
            //        Success = false,
            //        Message = "Входная модель оказалась пуста",
            //    };
            //}
        }
    }
}
