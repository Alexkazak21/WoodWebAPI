using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using WoodWebAPI.Data.Models.Order;

namespace WoodWebAPI.Worker.Controller.Commands;

public class NewOrderCommand : ICommand
{
    public TelegramBotClient Client => TelegramWorker.API;

    public string Name => "/new_order";

    public async Task Execute(Update update, CancellationToken cancellationToken)
    {
        if (!cancellationToken.IsCancellationRequested)
        {
            if(update != null && update.Type == UpdateType.CallbackQuery)
            {
                var chatid = update.CallbackQuery.From.Id;

                OrderModel[]? orderList = null;
                using (HttpClient client = new HttpClient())
                {
                    GetOrdersDTO getOrders = new GetOrdersDTO()
                    {
                        Customer_TelegramID = chatid.ToString(),
                    };
                    var content = JsonContent.Create(getOrders);

                    var responce = await client.PostAsync("http://localhost:5550/api/Order/GetOrdersOfCustomer", content, cancellationToken);

                    orderList = JsonConvert.DeserializeObject<OrderModel[]?>(await responce.Content.ReadAsStringAsync(cancellationToken));                    
                }

                if( orderList != null && orderList.Count() < 4)
                {
                    using (HttpClient client = new HttpClient())
                    {
                        CreateOrderDTO createOrder = new CreateOrderDTO()
                        {
                            Customer_Telegram_Id = chatid.ToString(),
                        };
                        var content = JsonContent.Create(createOrder);

                        var responce = await client.PostAsync("http://localhost:5550/api/Order/CreateOrder", content, cancellationToken);

                        if (responce.IsSuccessStatusCode)
                        {
                            var inlineMarkup = new InlineKeyboardMarkup(
                                new[]
                                {
                                    InlineKeyboardButton.WithCallbackData("К заказам", "/main")
                                });

                            await Client.SendTextMessageAsync(
                                chatId: chatid,
                                text: await responce.Content.ReadAsStringAsync(),
                                replyMarkup: inlineMarkup);
                        }
                    }
                }  
                else if (orderList != null && orderList.Count() == 4)
                {

                    var inlineMarkup = new InlineKeyboardMarkup(
                                new[]
                                {
                                    InlineKeyboardButton.WithCallbackData("К заказам", "/main")
                                });

                    await Client.SendTextMessageAsync(
                        chatId: chatid,
                        text: "Запрещено создавать более 4-ёх заказов одновременно." +
                        "\nОжидайте завершения предыдущих заказов.",
                        replyMarkup: inlineMarkup
                        );
                }
            }
        }
    }
}
