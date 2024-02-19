using Microsoft.EntityFrameworkCore;
using WoodWebAPI.Data;
using WoodWebAPI.Data.Entities;
using WoodWebAPI.Data.Models;
using WoodWebAPI.Data.Models.Timber;

namespace WoodWebAPI.Services;

public class TimberManageService : ITimberManage
{
    private readonly WoodDBContext _db;

    public TimberManageService(WoodDBContext db)
    {
        _db = db;
    }

    public async Task<GetTimber> GetFullOrderTimbersArrayAsync(GetTimberDTO model)
    {
        var customer = await _db.Customers.Where(x => x.TelegramID == model.customerTelegramId).FirstOrDefaultAsync();
        var order = await _db.Orders.Where(x => x.CustomerId == customer.CustomerId && x.Id == model.OrderId).Include(x => x.Timbers).FirstOrDefaultAsync();

        return
            new GetTimber()
            {
                OrderId = order.Id,
                Timbers = order.Timbers as List<Timber>,
            };
    }

    public async Task<ExecResultModel> AddTimberToOrderAsync(AddTimberDTO model)
    {
        var order = await _db.Orders.Where(x => x.Id == model.OrderId).Include(x => x.Timbers).FirstOrDefaultAsync();

        if (order != null)
        {
            try
            {
                if (model.Length > 6.9 || model.Length < 1.5)
                {
                    return new ExecResultModel()
                    {
                        Success = false,
                        Message = "Длина должна быть в диапазоне от 1.5m до 6.9m"
                    };
                }

                if (model.Diameter > 100 || model.Diameter < 14)
                {
                    return new ExecResultModel()
                    {
                        Success = false,
                        Message = "Диаметр должн быть в диапазоне от 14 см до 100 см"
                    };
                }

                int diameter = -1;
                double length = -1;

                // Проверка на существование введённого значения для длинны в базе

                if (!await _db.Kub.Where(item => item.Length == model.Length).AnyAsync())
                {
                    length = await _db.Kub.Where(item => item.Length > model.Length).MinAsync(item => item.Length);
                }
                else
                {
                    length = await _db.Kub.Where(item => item.Length == model.Length).Select(item => item.Length).FirstAsync();
                }

                // Проверка на существование введённого значения для диаметра в базе

                if (!await _db.Kub.Where(item => item.Diameter == model.Diameter).AnyAsync())
                {
                    diameter = await _db.Kub.Where(item => item.Diameter > model.Diameter).MinAsync(item => item.Diameter);
                }
                else
                {
                    diameter = await _db.Kub.Where(item => item.Diameter == model.Diameter).Select(item => item.Diameter).FirstAsync();
                }

                var volume = await _db.Kub.Where(item => item.Diameter == diameter && item.Length == length).Select(item => item.Value).FirstAsync();

                // сохранение данных о добавленном дереве  и изменившемся заказе в базу

                order.Timbers.Add(
                new Timber()
                {
                    Length = length,
                    Diameter = diameter,
                    Volume = volume,
                }
                );

                await _db.SaveChangesAsync();

                return new ExecResultModel()
                {
                    Success = true,
                    Message = "Бревно успешно добавлено"
                };
            }
            catch (Exception ex)
            {
                return new ExecResultModel()
                {
                    Success = false,
                    Message = "Что-то пошло не так, пожалуйста свяжитесь с администратором"
                };
            }
        }

        return new ExecResultModel()
        {
            Success = false,
            Message = "Указанный каказ не найден"
        };
    }

    public async Task<ExecResultModel> GetTotalVolumeOfOrderAsync(GetTimberDTO model)
    {
        if (model != null)
        {
            try
            {
                var customer = await _db.Customers.Where(x => x.TelegramID == model.customerTelegramId).FirstOrDefaultAsync();
                var order = await _db.Orders.Where(x => x.CustomerId == customer.CustomerId && x.Id == model.OrderId).Include(x => x.Timbers).FirstOrDefaultAsync();

                double totalVolume = 0.0;

                if (order != null)
                {
                    foreach (var timber in order.Timbers)
                    {
                        totalVolume += timber.Volume;
                    }
                }

                return new ExecResultModel()
                {
                    Success = true,
                    Message = $"{totalVolume}",
                };
            }
            catch (Exception ex)
            {
                return new ExecResultModel()
                {
                    Success = false,
                    Message = "Something went wrong, contact administrator"
                };
            }
        }

        return new ExecResultModel()
        {
            Success = false,
            Message = "no information specified"
        };
    }

    public async Task<ExecResultModel> UpdateTimberAsync(UpdateTimberDTO model)
    {
        if (model != null)
        {
            try
            {
                var order = await _db.Orders.Where(x => x.Id == model.OrderId).Include(x => x.Timbers).FirstOrDefaultAsync();

                if (order != null)
                {
                    foreach (var timber in order.Timbers)
                    {
                        if (timber.Id == model.TimberId)
                        {
                            timber.Diameter = model.Diameter;
                            timber.Length = model.Length;
                        }
                    }

                   await _db.SaveChangesAsync();
                }

                return new ExecResultModel()
                {
                    Success = true,
                    Message = $"Timber {model.TimberId} was updated",
                };
            }
            catch (Exception ex)
            {
                return new ExecResultModel()
                {
                    Success = false,
                    Message = "Something went wrong, contact administrator"
                };
            }
        }

        return new ExecResultModel()
        {
            Success = false,
            Message = "no information specified"
        };
    }

    public async Task<GetTimberArray> GetTimberArrayAsync(GetTimberByOrderDTO model)
    {
        var order = await _db.Orders.Where(x => x.Id == model.OrderId).Include(x => x.Timbers).FirstOrDefaultAsync();

        List<GetTimberArrayDTO> getTimbers = new List<GetTimberArrayDTO>();

        var timbersArray = order.Timbers.ToArray();


        for (int i = 0; i < timbersArray.Length; i++)
        {
            getTimbers.Add(new GetTimberArrayDTO()
            {
                TimberNumber = timbersArray[i].Id,
                Diameter = timbersArray[i].Diameter,
                Length = timbersArray[i].Length,
            });
        }

        return
            new GetTimberArray()
            {
                OrderId = order.Id,
                Timbers = getTimbers,
            };
    }
}
