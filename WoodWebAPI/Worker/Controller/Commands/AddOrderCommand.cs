using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using WoodWebAPI.Data.Models;
using WoodWebAPI.Data.Models.Order;

namespace WoodWebAPI.Worker.Controller.Commands;

public class AddOrderCommand(IWorkerCreds workerCreds) : ICommand
{
    private readonly IWorkerCreds _workerCreds = workerCreds;
    public TelegramBotClient Client => TelegramWorker.API;

    public string Name => "/new_order";

    public async Task Execute(Update update, CancellationToken cancellationToken)
    {
        if (!cancellationToken.IsCancellationRequested)
        {
            if (update != null && update.Type == UpdateType.CallbackQuery)
            {
                var chatid = update.CallbackQuery.From.Id;

                var messageid = update.CallbackQuery.Message.MessageId;

                OrderModel[]? orderList = null;
                using (HttpClient client = new HttpClient())
                {
                    GetOrdersDTO getOrders = new GetOrdersDTO()
                    {
                        CustomerTelegramId = chatid,
                    };
                    var content = JsonContent.Create(getOrders);

                    var responce = await client.PostAsync($"{_workerCreds.BaseURL}/api/Order/GetOrdersOfCustomer", content, cancellationToken);

                    orderList = JsonConvert.DeserializeObject<OrderModel[]?>(await responce.Content.ReadAsStringAsync(cancellationToken));
                }

                if (orderList != null && orderList.Count() < 4)
                {
                    using (HttpClient client = new HttpClient())
                    {
                        GetOrdersDTO createOrder = new()
                           {
                                CustomerTelegramId = chatid,
                           };
                        var content = JsonContent.Create(createOrder);

                        var request = await client.PostAsync("http://localhost:5550/api/Order/CreateOrder", content, cancellationToken);

                        var response = JsonConvert.DeserializeObject<ExecResultModel>(await request.Content.ReadAsStringAsync());
                        if (request.IsSuccessStatusCode)
                        {
                            var inlineMarkup = new InlineKeyboardMarkup(
                                new[]
                                {
                                    InlineKeyboardButton.WithCallbackData("К заказам", "/main")
                                });

                            await Client.EditMessageTextAsync(
                                chatId: chatid,
                                text: response.Message,
                                messageId: messageid,
                                replyMarkup: inlineMarkup,
                                cancellationToken: cancellationToken);
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


                    await Client.EditMessageTextAsync(
                        chatId: chatid,
                        text: "Запрещено создавать более 4-ёх заказов одновременно." +
                        "\nОжидайте завершения предыдущих заказов.",
                        messageId: messageid,
                        replyMarkup: inlineMarkup
                        );
                }
            }
        }
    }
}
