using Microsoft.EntityFrameworkCore;
using WoodWebAPI.Data;
using WoodWebAPI.Data.Entities;
using WoodWebAPI.Data.Models;
using WoodWebAPI.Data.Models.OrderPosition;

namespace WoodWebAPI.Services;

public class OrderPositionManageService : IOrderPositionManage
{
    private readonly WoodDBContext _db;

    public OrderPositionManageService(WoodDBContext db)
    {
        _db = db;
    }

    public async Task<OrderPositionsModel> GetOrderPositionsOfOrderAsync(GetOrderPositionsByOrderIdDTO model)
    {
        try
        {
            var order = await _db.Orders
                .Where(x => x.CustomerTelegramId == model.TelegramId && x.Id == model.OrderId)
                .Include(x => x.OrderPositions)
                .Select(x => new OrderPositionsModel
                {
                    OrderId = x.Id,
                    OrderPositions = x.OrderPositions.Select(y => new OrderPositionDTO
                    {
                        OrderPositionId = y.Id,
                        DiameterInCantimeter = y.DiameterInCantimeter,
                        LengthInMeter = y.LengthInMeter,
                        VolumeInMeter3 = y.VolumeInMeter3
                    }).ToList()
                })
                .FirstAsync();

            return order;

        }
        catch (ArgumentNullException)
        {
            return new();
        }
    }

    public async Task<ExecResultModel> AddOrderPositionToOrderAsync(AddOrderPositionDTO model)
    {
        if (model == null)
        {
            return new ExecResultModel()
            {
                Success = false,
                Message = "Входные данные отсутствуют"
            };
        }

        decimal minLength = 0m, maxLength = 0m, minDiameter = 0m, maxDiameter = 0m;
        try
        {
            minLength = await _db.EtalonTimberList.Select(x => x.LengthInMeter).MinAsync();
            maxLength = await _db.EtalonTimberList.Select(x => x.LengthInMeter).MaxAsync();
            minDiameter = await _db.EtalonTimberList.Select(x => x.DiameterInСantimeter).MinAsync();
            maxDiameter = await _db.EtalonTimberList.Select(x => x.DiameterInСantimeter).MaxAsync();

            var etalonOrderPosition = await _db.EtalonTimberList
                .Where(x => x.LengthInMeter >= model.Length)
                .Where(x => x.DiameterInСantimeter >= model.Diameter)
                .OrderBy(x => x.LengthInMeter)
                .ThenBy(x => x.DiameterInСantimeter)
                .ThenBy(x => x.VolumeInMeter3)
                .FirstOrDefaultAsync() ?? throw new ArgumentNullException();

            // сохранение данных о добавленном дереве  и изменившемся заказе в базу

            var timber = await _db.OrderPositions.AddAsync(
                new OrderPosition
                {
                    OrderId = model.OrderId,
                    DiameterInCantimeter = etalonOrderPosition.DiameterInСantimeter,
                    LengthInMeter = etalonOrderPosition.LengthInMeter,
                    VolumeInMeter3 = etalonOrderPosition.VolumeInMeter3,
                });

            await _db.SaveChangesAsync();

            return new ExecResultModel()
            {
                Success = true,
                Message = "Бревно успешно добавлено"
            };
        }
        catch (ArgumentNullException)
        {
            return new ExecResultModel()
            {
                Success = false,
                Message = "Что-то пошло не так, проверьте правильность введённых данных и попробуйте снова\n" +
                $"Длинна в диапазоне от {minLength:0.00}м до {maxLength:0.00}м\n" +
                $"Диаметр в диапазоне от {(int)minDiameter}см до {(int)maxDiameter}см"
            };
        }
        catch (DbUpdateException)
        {
            return new ExecResultModel()
            {
                Success = false,
                Message = "Ошибка в БД, попробуйте позже"
            };
        }
    }

    public async Task<double> GetTotalVolumeOfOrderAsync(GetOrderPositionsByOrderIdDTO model)
    {
        if (model == null)
        {
            return double.NegativeZero;
        }

        try
        {
            double totalVolume = await _db.Orders
                .Where(x => x.CustomerTelegramId == model.TelegramId && x.Id == model.OrderId).Include(x => x.OrderPositions)
                .Select(x => x.OrderPositions)
                .Select(x => x.Sum(y => y.VolumeInMeter3)).FirstAsync();

            return totalVolume;
        }
        catch (ArgumentNullException)
        {
            return double.NegativeZero;
        }
    }

    public async Task<ExecResultModel> UpdateOrderPositionAsync(UpdateOrderPositionDTO model)
    {
        if (model == null)
        {
            return new ExecResultModel()
            {
                Success = false,
                Message = "Нет входных данных"
            };
        }

        try
        {
            var order = await _db.Orders
                .Where(x => x.Id == model.OrderId)
                .Include(x => x.OrderPositions)
                .Select(x => x.OrderPositions.Where(y => y.Id == model.OrderPositionId).First())
                .FirstAsync();

            _db.OrderPositions.Remove(order);

            await AddOrderPositionToOrderAsync(new AddOrderPositionDTO
            {
                Diameter = model.DiameterInCantimeter,
                Length = model.LengthInMeter,
                OrderId = model.OrderId,
            });

            await _db.SaveChangesAsync();

            return new ExecResultModel()
            {
                Success = true,
                Message = $"Бревно {model.OrderPositionId} было обновлено",
            };
        }
        catch (ArgumentNullException)
        {
            return new ExecResultModel()
            {
                Success = false,
                Message = "Указанное бревно не найдено"
            };
        }
        catch (DbUpdateException)
        {
            return new ExecResultModel()
            {
                Success = false,
                Message = "Ошибка в БД, попробуйте позже"
            };
        }
    }
}
