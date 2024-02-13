using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using WoodWebAPI.Data.Models.Customer;
using WoodWebAPI.Data.Models.Order;

namespace WoodWebAPI.Worker.Controller.Commands;

public class OrderManageCommand : ICommand
{
    public TelegramBotClient Client => TelegramWorker.API;

    public string Name => "/order_manage";

    public async Task Execute(Update update, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested) { return; }

        if(update != null) 
        {
            var chatId = 0l;
            string[]? commandParts = null;
            var messageid = -1;
            if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message)
            {
                chatId = update.Message.From.Id;
            }
            else if(update.Type == Telegram.Bot.Types.Enums.UpdateType.CallbackQuery) 
            {
                chatId = update.CallbackQuery.From.Id;
                commandParts = update.CallbackQuery.Data.Split(":");
                messageid = update.CallbackQuery.Message.MessageId;
            }

            if(commandParts != null && commandParts.Length > 1) 
            {
                if (commandParts[1] == "all")
                {
                    if (commandParts.Length > 2 && commandParts[2] != null)
                    {
                        await ShowAllOrders(update, int.Parse(commandParts[2]));
                    }
                    else
                    {
                        await ShowAllOrders(update);
                    }
                }
                else if(commandParts.Length > 2 && commandParts[1] == "true")
                {
                    if (commandParts[2] != null)
                    {
                        await ShowAllOrders(update, int.Parse(commandParts[2]));
                    }
                    else
                    {
                        await ShowVerifiedOrders(update);
                    }
                }
                else if (commandParts[1] == "false")
                {
                    if (commandParts.Length > 2 && commandParts[2] != null)
                    {
                        await ShowAllOrders(update, int.Parse(commandParts[2]));
                    }
                    else
                    {
                        await ShowNotVerifiedOrders(update);
                    }                    
                }
                else
                {
                    TelegramWorker.Logger.LogWarning("Wrong param while proccessing order manage command");
                }
            }
            else
            {
                InlineKeyboardMarkup replyMarkup = new InlineKeyboardMarkup(
                    new[]
                    {
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("Весь список заказов","/order_manage:all")
                        },
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("Заказы в работе","/order_manage:true")
                        },
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("Ожидают подтверждение","/order_manage:false")
                        },
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("Главное меню","/main"),
                        }
                    });
                if (messageid > 0)
                {
                    await Client.EditMessageTextAsync(
                        chatId: chatId,
                        messageId: messageid,
                        text: "Выберите нужное действие",
                        replyMarkup: replyMarkup,
                        cancellationToken: cancellationToken
                    );
                }
                else
                {
                    await Client.SendTextMessageAsync(
                        chatId: chatId,
                        text: "Выберите нужное действие",
                        replyMarkup: replyMarkup,
                        cancellationToken: cancellationToken
                    );
                }
                
            }
        }
    }

    private async Task ShowAllOrders(Update update, int? orderId = null, CancellationToken cancellationToken = default)
    {
        if( cancellationToken.IsCancellationRequested ) return;

        if( update != null )
        {
            var customerId = -1l;

            var messageId = update.CallbackQuery.Message.MessageId;

            bool approved = false;

            List<OrderModel>? orders = null;

            if ( update.Type == Telegram.Bot.Types.Enums.UpdateType.Message ) 
            {
                customerId = update.Message.From.Id;
                approved = await new CommonChecks().CheckCustomer(customerId, cancellationToken);
            }
            else if( update.Type == Telegram.Bot.Types.Enums.UpdateType.CallbackQuery )
            {
                customerId = update.CallbackQuery.From.Id;
                approved = true;
            }

            var chatId = customerId;

            if(approved)
            {
                var customersList = new List<GetCustomerModel>();
            }

            using (HttpClient httpClient = new HttpClient())
            {
                var content = new StringContent("");

                var responce = await httpClient.PostAsync($"{TelegramWorker.BaseUrl}/api/Order/GetFullOrdersList", content, cancellationToken);
                var responseJsonContent = await responce.Content.ReadAsStringAsync(cancellationToken);
                orders = JsonConvert.DeserializeObject<OrderModel[]?>(responseJsonContent).ToList();
            }

            if(orders == null || orders.Count == 0)
            {
                await Client.EditMessageTextAsync(
                        chatId: chatId,
                        messageId: messageId,
                        text: "Пока нет никаких заказов (",
                        replyMarkup: new InlineKeyboardMarkup(
                            [
                                InlineKeyboardButton.WithCallbackData("Главное меню", "/main"),
                            ]),
                        cancellationToken: cancellationToken
                    );
            }
            else if(orders != null && orderId == null)
            {
                
                if(orders.Count == 1)
                {
                    await Client.EditMessageTextAsync(
                        chatId: chatId,
                        messageId: messageId,
                        text: $"Заказ № {orders[0].Id}\n" +
                        $"Создан {orders[0].CreatedAt}\n" +
                        $"Подтверждён: {(orders[0].IsVerified == false ? "Нет" : "Да")}\n" +
                        $"Завершён: {(orders[0].IsCompleted == false ? "Нет" : "Да")}\n",
                        replyMarkup: new InlineKeyboardMarkup(
                            [
                                InlineKeyboardButton.WithCallbackData("Главное меню", "/main"),
                            ]),
                        cancellationToken: cancellationToken
                        );
                }
                else
                {
                    await Client.EditMessageTextAsync(
                        chatId: chatId,
                        messageId: messageId,
                        text: $"Заказ № {orders[0].Id}\n" +
                        $"Создан {orders[0].CreatedAt}\n" +
                        $"Подтверждён: {(orders[0].IsVerified == false ? "Нет" : "Да")}\n" +
                        $"Завершён: {(orders[0].IsCompleted == false ? "Нет" : "Да")}\n",
                        replyMarkup: new InlineKeyboardMarkup(
                           new[]
                           {
                               new[]
                               {
                                    InlineKeyboardButton.WithCallbackData(">", $"/order_manage:all:{orders[1].Id}"),
                               },
                               new[]
                               {                  
                                   InlineKeyboardButton.WithCallbackData("Главное меню", "/main"),
                               }
                           }),
                        cancellationToken: cancellationToken
                        );
                }
                    
            }
            else if (orders != null && orderId != null)
            {
                InlineKeyboardMarkup replyMarkup = null;

                if (orders[0].Id == orderId)
                {
                    replyMarkup = new InlineKeyboardMarkup(
                           new[]
                           {
                               new[]
                               {
                                    InlineKeyboardButton.WithCallbackData(">", $"/order_manage:all:{orders[1].Id}"),
                               },
                               new[]
                               {
                                   InlineKeyboardButton.WithCallbackData("Главное меню", "/main"),
                               }
                           });
                }
                else if (orders.Count > 2 && orders[^1].Id == orderId)
                {
                    replyMarkup = new InlineKeyboardMarkup(
                           new[]
                           {
                               new[]
                               {
                                    InlineKeyboardButton.WithCallbackData("<", $"/order_manage:all:{orders[^2].Id}"),
                               },
                               new[]
                               {
                                   InlineKeyboardButton.WithCallbackData("Главное меню", "/main"),
                               }
                           });
                }
                else
                {
                    replyMarkup = new InlineKeyboardMarkup(
                           new[]
                           {
                               new[]
                               {
                                    InlineKeyboardButton.WithCallbackData("<", $"/order_manage:all:{orders[orders.IndexOf(orders.Where(x => x.Id == orderId).First()) - 1].Id}"),
                                    InlineKeyboardButton.WithCallbackData(">", $"/order_manage:all:{orders[orders.IndexOf(orders.Where(x => x.Id == orderId).First()) + 1].Id}"),
                               },
                               new[]
                               {
                                   InlineKeyboardButton.WithCallbackData("Главное меню", "/main"),
                               }
                           });
                }

                await Client.EditMessageTextAsync(
                        chatId: chatId,
                        messageId: messageId,
                        text: $"Заказ № {orders[orders.IndexOf(orders.Where(x => x.Id == orderId).First())].Id}\n" +
                        $"Создан {orders[orders.IndexOf(orders.Where(x => x.Id == orderId).First())].CreatedAt}\n" +
                        $"Подтверждён: {(orders[orders.IndexOf(orders.Where(x => x.Id == orderId).First())].IsVerified == false ? "Нет" : "Да")}\n" +
                        $"Завершён: {(orders[orders.IndexOf(orders.Where(x => x.Id == orderId).First())].IsCompleted == false ? "Нет" : "Да")}\n",
                        replyMarkup: replyMarkup,
                        cancellationToken: cancellationToken
                        );
            }
        }

        return;
    }

    private async Task ShowVerifiedOrders(Update update, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested) return;
    }

    private async Task ShowNotVerifiedOrders(Update update, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested) return;
    }
}
