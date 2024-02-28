using WoodWebAPI.Data.Entities;
using WoodWebAPI.Data.Models.Order;

namespace WoodWebAPI.Services.Extensions;

public static class OrderExtensions
{
    public static bool ChangeStatus(this Order order, OrderStatus newStatus)
    {
        return order.Status switch
        {
            OrderStatus.NewOrder => newStatus == OrderStatus.Approved || newStatus == OrderStatus.Archived,
            OrderStatus.Approved => newStatus == OrderStatus.Verivied  || newStatus == OrderStatus.CanceledByAdmin,
            OrderStatus.Verivied => newStatus == OrderStatus.Completed || newStatus == OrderStatus.CanceledByAdmin,
            OrderStatus.CanceledByAdmin => newStatus == OrderStatus.Archived,
            OrderStatus.Completed => newStatus == OrderStatus.Paid,
            OrderStatus.Paid => newStatus == OrderStatus.Archived,
            _ => false,
        };
    }

    public static string OrderStatusMessage(this OrderModel? order) 
    {
        if (order == null) return string.Empty;

        return order.Status switch
        {
            OrderStatus.NewOrder => "Новый заказ, не подтверждён администратором",
            OrderStatus.Approved => "Подтверждён",
            OrderStatus.Verivied => "Принят в работу",
            OrderStatus.Completed => "Завершён, ожидает оплату",
            OrderStatus.Archived => "В архиве",
            _ => "На рассмотрении Администратора"
        };
    }
}
