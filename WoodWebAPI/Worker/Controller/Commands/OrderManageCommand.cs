using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using WoodWebAPI.Data.Models;
using WoodWebAPI.Data.Models.Customer;
using WoodWebAPI.Data.Models.Order;
using WoodWebAPI.Data.Entities;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace WoodWebAPI.Worker.Controller.Commands;

public class OrderManageCommand : ICommand
{
    public TelegramBotClient Client => TelegramWorker.API;

    public string Name => "/order_manage";

    public async Task Execute(Update update, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested) { return; }

        if (update != null)
        {
            var chatId = 0l;
            string[]? commandParts = null;
            var messageid = -1;
            if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message)
            {
                chatId = update.Message.From.Id;
            }
            else if (update.Type == Telegram.Bot.Types.Enums.UpdateType.CallbackQuery)
            {
                chatId = update.CallbackQuery.From.Id;
                commandParts = update.CallbackQuery.Data.Split(":");
                messageid = update.CallbackQuery.Message.MessageId;
            }

            if (commandParts != null && commandParts.Length > 1)
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
                else if (commandParts[1] == "true")
                {
                    if (commandParts.Length > 2 && commandParts[2] != null)
                    {
                        await ShowAllOrders(update, int.Parse(commandParts[2]), verified: true);
                    }
                    else
                    {
                        await ShowAllOrders(update, verified: true);
                    }
                }
                else if (commandParts[1] == "false")
                {
                    if (commandParts.Length > 2 && commandParts[2] != null)
                    {
                        await ShowAllOrders(update, int.Parse(commandParts[2]), verified: false);
                    }
                    else
                    {
                        await ShowAllOrders(update, verified: false);
                    }
                }
                else if (commandParts[1] == "complete")
                {
                    if (commandParts.Length > 2 && commandParts[2] != null)
                    {
                        var result = await CompleteOrderAsync(update, int.Parse(commandParts[2]));

                        await Client.EditMessageTextAsync(
                        chatId: chatId,
                        messageId: messageid,
                        text: $"{result.Message}",
                        replyMarkup: new InlineKeyboardMarkup(
                            new[]
                            {
                               InlineKeyboardButton.WithCallbackData("Назад","/order_manage"),
                            }
                            ),
                        cancellationToken: cancellationToken
                    );
                    }
                }
                else if (commandParts.Length > 2 && commandParts[1] == "verify")
                {
                    var result = await VerifyOrderAsync(update, int.Parse(commandParts[2]));

                    await Client.EditMessageTextAsync(
                       chatId: chatId,
                       messageId: messageid,
                       text: $"{result.Message}",
                       replyMarkup: new InlineKeyboardMarkup(
                           new[]
                           {
                               InlineKeyboardButton.WithCallbackData("Назад","/order_manage"),
                           }
                           ),
                       cancellationToken: cancellationToken
                   );
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
                            InlineKeyboardButton.WithCallbackData("Ожидают подтверждение","/order_manage:false")
                        },
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("Заказы в работе","/order_manage:true")
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

    private async Task<ExecResultModel> CompleteOrderAsync(Update update, int orderId)
    {
        var content = JsonContent.Create(new VerifyOrderDTO
        {
            OrderId = orderId,
        });
        ExecResultModel completeResult = null;
        using (HttpClient httpClient = new HttpClient())
        {
            var request = await httpClient.PostAsync($"{TelegramWorker.BaseUrl}/api/Order/CompleteOrderByAdmin", content);
            var responce = await request.Content.ReadAsStringAsync();
            completeResult = JsonConvert.DeserializeObject<ExecResultModel>(responce);
        }

        if (completeResult.Success)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                var responce = await httpClient.PostAsync($"{TelegramWorker.BaseUrl}/api/Customer/GetCustomers", new StringContent(""));
                var responseJsonContent = await responce.Content.ReadAsStringAsync();
                GetCustomerModel[] customersArray = JsonConvert.DeserializeObject<GetCustomerModel[]>(responseJsonContent);

                foreach (var customer in customersArray)
                {
                    if (customer.Orders.FirstOrDefault(x => x.Id == orderId) != null)
                    {
                        var volume = await new CommonChecks().GetVolume(customer.TelegramId, orderId);

                        var ammountToPay = decimal.Round(TelegramWorker.PriceForM3 * decimal.Parse(volume.Message), 2, MidpointRounding.AwayFromZero) > TelegramWorker.MinPrice ?
                                            decimal.Round(TelegramWorker.PriceForM3 * decimal.Parse(volume.Message), 2, MidpointRounding.AwayFromZero) : TelegramWorker.MinPrice;
                        await Client.SendTextMessageAsync(
                        chatId: customer.TelegramId,
                        text: $"{completeResult.Message}\n" +
                        $"Произведите оплату в размере {ammountToPay} бел. рублей",
                        replyMarkup: new InlineKeyboardMarkup(
                        new[]
                        {
                                new[]
                                {
                                    InlineKeyboardButton.WithCallbackData("Перейти к оплате", $"/payment:{customer.TelegramId}:{ammountToPay}:{orderId}"),
                                }
                        })
                        );
                    }
                }
            }
            var customers = completeResult;
        }
        return completeResult;
    }
    private async Task ShowAllOrders(Update update, int? orderId = null, bool? verified = null, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested) return;

        if (update != null)
        {
            var customerId = -1l;

            var messageId = update.CallbackQuery.Message.MessageId;

            bool CustomerIsAdmin = false;

            List<OrderModel>? orders = null;

            if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message)
            {
                customerId = update.Message.From.Id;
                CustomerIsAdmin = await new CommonChecks().CheckCustomer(customerId, cancellationToken);
            }
            else if (update.Type == Telegram.Bot.Types.Enums.UpdateType.CallbackQuery)
            {
                customerId = update.CallbackQuery.From.Id;
                CustomerIsAdmin = true;
            }

            var chatId = customerId;

            if (CustomerIsAdmin)
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

            if (verified == true)
            {
                orders = orders.Where(x => x.Status == OrderStatus.Verivied).ToList();
                await SendTemplateAsync(orders, chatId, messageId, verified: true, orderId: orderId, cancellationToken: cancellationToken);
            }
            else if (verified == false)
            {
                orders = orders.Where(x => x.Status == OrderStatus.NewOrder).ToList();
                await SendTemplateAsync(orders, chatId, messageId, verified: false, orderId: orderId, cancellationToken: cancellationToken);
            }
            else
            {
                await SendTemplateAsync(orders, chatId, messageId, orderId: orderId, cancellationToken: cancellationToken);
            }

        }

        return;
    }

    private async Task<ExecResultModel> VerifyOrderAsync(Update update, int orderId, CancellationToken cancellationToken = default)
    {
        var content = JsonContent.Create(new VerifyOrderDTO
        {
            OrderId = orderId,
        });

        using (HttpClient httpClient = new HttpClient())
        {
            var request = await httpClient.PostAsync($"{TelegramWorker.BaseUrl}/api/Order/VerifyOrderByAdmin", content);
            var responce = await request.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<ExecResultModel>(responce);
            return result;
        }
    }

    private async Task SendTemplateAsync(List<OrderModel>? orders, long chatId, int messageId, bool? verified = null, int? orderId = null, CancellationToken cancellationToken = default)
    {

        bool isVerified = false;
        bool isCompleted = false;
        bool isPaid = false;

        if(orders != null && orderId != null) 
        {
            orderId = orders[0].Id;

            switch (orders[orders.IndexOf(orders.Where(x => x.Id == orderId).First())].Status)
            {
                case OrderStatus.Verivied:
                    {
                        isVerified = true;
                    }
                    break;
                case OrderStatus.Completed:
                    {
                        isVerified = true;
                        isCompleted = true;
                    }
                    break;
                case OrderStatus.Paid:
                    {
                        isPaid = true;
                        isCompleted = true;
                        isVerified = true;
                    }
                    break;

            }
        }

        // список заказов пуст
        if (orders == null || orders.Count == 0)
        {
            try
            {
                await Client.EditMessageTextAsync(
                    chatId: chatId,
                    messageId: messageId,
                    text: "Пока нет никаких заказов (",
                    replyMarkup: new InlineKeyboardMarkup(
                        new[]
                        {
                                new[]
                                {
                                    InlineKeyboardButton.WithCallbackData("Назад", "/order_manage"),
                                },
                                new[]
                                {
                                    InlineKeyboardButton.WithCallbackData("Главное меню", "/main"),
                                }
                        }),
                    cancellationToken: cancellationToken
                );
            }
            catch (Telegram.Bot.Exceptions.ApiRequestException ex)
            {
                TelegramWorker.Logger.LogError(ex.Message);
            }
        }

        InlineKeyboardButton proccessButton = new("");
        // установка корректной кнопки действия с заказом
        if (orders.Count > 0)
        {
            if (orderId != null)
            {
                if (isVerified == false)
                {
                    proccessButton = InlineKeyboardButton.WithCallbackData("Приять в работу", $"/order_manage:verify:{orders[orders.IndexOf(orders.Where(x => x.Id == orderId).First())].Id}");
                }
                else
                {
                    proccessButton = InlineKeyboardButton.WithCallbackData("Завершить заказ", $"/order_manage:complete:{orders[orders.IndexOf(orders.Where(x => x.Id == orderId).First())].Id}");
                }
            }
            else
            {
                if (isVerified == false)
                {
                    proccessButton = InlineKeyboardButton.WithCallbackData("Приять в работу", $"/order_manage:verify:{orders[0].Id}");
                }
                else
                {
                    proccessButton = InlineKeyboardButton.WithCallbackData("Завершить заказ", $"/order_manage:complete:{orders[0].Id}");
                }
            }

            // отображение целого списк заказов без навигации, без признака Verified || Verified = false
            if (orders != null && orderId == null && (verified == null || verified == false))
            {
                var volumeTotal = await new CommonChecks().GetVolume(chatId, orders[0].Id);
                if (orders.Count == 1)
                {
                    await Client.EditMessageTextAsync(
                        chatId: chatId,
                        messageId: messageId,
                        text: $"Заказ № {orders[0].Id}\n" +
                        $"Создан {orders[0].CreatedAt}\n" +
                        $"Объёмом: {volumeTotal.Message} m3\n" +
                        $"Подтверждён: {(isVerified == false ? "Нет" : "Да")}\n" +
                        $"Завершён: {(isCompleted == false ? "Нет" : "Да")}\n" +
                        $"Оплачен: {(isPaid == false ? "Нет" : "Да")}",
                        replyMarkup: new InlineKeyboardMarkup(
                            new[]
                            {
                                new[]
                                {
                                   proccessButton,
                                },
                                new[]
                                {
                                    InlineKeyboardButton.WithCallbackData("Назад", "/order_manage"),
                                    InlineKeyboardButton.WithCallbackData("Главное меню", "/main"),
                                }
                            }),
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
                        $"Объёмом: {volumeTotal.Message} m3\n" +
                        $"Подтверждён: {(isVerified == false ? "Нет" : "Да")}\n" +
                        $"Завершён: {(isCompleted == false ? "Нет" : "Да")}\n" +
                        $"Оплачен: {(isPaid == false ? "Нет" : "Да")}",
                        replyMarkup: new InlineKeyboardMarkup(
                           new[]
                           {
                               new[]
                               {
                                   InlineKeyboardButton.WithCallbackData(">", $"/order_manage:all:{orders[1].Id}"),
                               },
                               new[]
                               {
                                   proccessButton,
                               },
                               new[]
                               {
                                   InlineKeyboardButton.WithCallbackData("Назад", "/order_manage"),
                                   InlineKeyboardButton.WithCallbackData("Главное меню", "/main"),
                               }
                           }),
                        cancellationToken: cancellationToken
                    );
                }

            }

            // отображение целого списк заказов с навигацией, без признака Verified || Verified = false
            else if (orders != null && orderId != null && (verified == null || verified == false))
            {
                var volumeTotal = await new CommonChecks().GetVolume(chatId, (int)orderId);
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
                                   proccessButton,
                               },
                               new[]
                               {
                                   InlineKeyboardButton.WithCallbackData("Назад", "/order_manage"),
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
                                   proccessButton,
                               },
                               new[]
                               {
                                   InlineKeyboardButton.WithCallbackData("Назад", "/order_manage"),
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
                                   proccessButton,
                               },
                               new[]
                               {
                                   InlineKeyboardButton.WithCallbackData("Назад", "/order_manage"),
                                   InlineKeyboardButton.WithCallbackData("Главное меню", "/main"),
                               }
                           });
                }

                await Client.EditMessageTextAsync(
                        chatId: chatId,
                        messageId: messageId,
                        text: $"Заказ № {orders[orders.IndexOf(orders.Where(x => x.Id == orderId).First())].Id}\n" +
                              $"Создан {orders[orders.IndexOf(orders.Where(x => x.Id == orderId).First())].CreatedAt}\n" +
                              $"Объёмом: {volumeTotal.Message} m3\n" +
                              $"Подтверждён: {(isVerified == false ? "Нет" : "Да")}\n" +
                              $"Завершён: {(isCompleted == false ? "Нет" : "Да")}\n" +
                               $"Оплачен: {(isPaid == false ? "Нет" : "Да")}",
                        replyMarkup: replyMarkup,
                        cancellationToken: cancellationToken
                        );
            }

            //отображение списка заказов без навигации, признак Verified = true
            else if (orders != null && orderId == null && verified == true)
            {
                var volumeTotal = await new CommonChecks().GetVolume(chatId, orders[0].Id);

                if (orders.Count == 1)
                {
                    await Client.EditMessageTextAsync(
                        chatId: chatId,
                        messageId: messageId,
                        text: $"Заказ № {orders[0].Id}\n" +
                        $"Создан {orders[0].CreatedAt}\n" +
                        $"Объёмом: {volumeTotal.Message} m3\n" +
                        $"Подтверждён: {(isVerified == false ? "Нет" : "Да")}\n" +
                        $"Завершён: {(isCompleted == false ? "Нет" : "Да")}\n" +
                        $"Оплачен: {(isPaid == false ? "Нет" : "Да")}",
                        replyMarkup: new InlineKeyboardMarkup(
                            new[]
                            {
                                new[]
                                {
                                    InlineKeyboardButton.WithCallbackData("Завершить заказ", $"/order_manage:complete:{orders[0].Id}"),
                                },
                                new[]
                                {
                                    InlineKeyboardButton.WithCallbackData("Назад", "/order_manage"),
                                    InlineKeyboardButton.WithCallbackData("Главное меню", "/main"),
                                }
                            }),
                        cancellationToken: cancellationToken
                        );
                }
                else
                {
                    InlineKeyboardButton rightButton = new("");
                    if (verified == true)
                    {
                        rightButton = InlineKeyboardButton.WithCallbackData(">", $"/order_manage:true:{orders[1].Id}");
                    }
                    else
                    {
                        rightButton = InlineKeyboardButton.WithCallbackData(">", $"/order_manage:all:{orders[1].Id}");
                    }

                    await Client.EditMessageTextAsync(
                        chatId: chatId,
                        messageId: messageId,
                        text: $"Заказ № {orders[0].Id}\n" +
                        $"Создан {orders[0].CreatedAt}\n" +
                        $"Объёмом: {volumeTotal.Message} m3\n" +
                        $"Подтверждён: {(isVerified == false ? "Нет" : "Да")}\n" +
                        $"Завершён: {(isCompleted == false ? "Нет" : "Да")}\n" +
                        $"Оплачен: {(isPaid == false ? "Нет" : "Да")}",
                        replyMarkup: new InlineKeyboardMarkup(
                           new[]
                           {
                               new[]
                               {
                                   rightButton,
                               },
                               new[]
                               {
                                   proccessButton,
                               },
                               new[]
                               {
                                   InlineKeyboardButton.WithCallbackData("Назад", "/order_manage"),
                                   InlineKeyboardButton.WithCallbackData("Главное меню", "/main"),
                               }
                           }),
                        cancellationToken: cancellationToken
                    );
                }
            }

            // отображение списка заказов с навигацией, признак Verified = true
            else if (orders != null && orderId != null && verified == true)
            {
                var volumeTotal = await new CommonChecks().GetVolume(chatId, (int)orderId);

                InlineKeyboardMarkup replyMarkup = null;

                if (orders[0].Id == orderId)
                {
                    replyMarkup = new InlineKeyboardMarkup(
                           new[]
                           {
                               new[]
                               {
                                    InlineKeyboardButton.WithCallbackData(">", $"/order_manage:true:{orders[1].Id}"),
                               },
                               new[]
                               {
                                   proccessButton,
                               },
                               new[]
                               {
                                   InlineKeyboardButton.WithCallbackData("Назад", "/order_manage"),
                                   InlineKeyboardButton.WithCallbackData("Главное меню", "/main"),
                               }
                           });
                }
                else if (orders.Count >= 2 && orders[^1].Id == orderId)
                {
                    replyMarkup = new InlineKeyboardMarkup(
                           new[]
                           {
                               new[]
                               {
                                    InlineKeyboardButton.WithCallbackData("<", $"/order_manage:true:{orders[^2].Id}"),
                               },
                               new[]
                               {
                                   proccessButton,
                               },
                               new[]
                               {
                                   InlineKeyboardButton.WithCallbackData("Назад", "/order_manage"),
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
                                   InlineKeyboardButton.WithCallbackData("<", $"/order_manage:true:{orders[orders.IndexOf(orders.Where(x => x.Id == orderId).First()) - 1].Id}"),
                                   InlineKeyboardButton.WithCallbackData(">", $"/order_manage:true:{orders[orders.IndexOf(orders.Where(x => x.Id == orderId).First()) + 1].Id}"),
                               },
                               new[]
                               {
                                   proccessButton,
                               },
                               new[]
                               {
                                   InlineKeyboardButton.WithCallbackData("Назад", "/order_manage"),
                                   InlineKeyboardButton.WithCallbackData("Главное меню", "/main"),
                               }
                           });
                };
                
                await Client.EditMessageTextAsync(
                        chatId: chatId,
                        messageId: messageId,
                        text: $"Заказ № {orders[orders.IndexOf(orders.Where(x => x.Id == orderId).First())].Id}\n" +
                              $"Создан {orders[orders.IndexOf(orders.Where(x => x.Id == orderId).First())].CreatedAt}\n" +
                              $"Объёмом: {volumeTotal.Message} m3\n" +
                              $"Подтверждён: {(isVerified == false ? "Нет" : "Да")}\n" +
                              $"Завершён: {(isCompleted == false ? "Нет" : "Да")}\n" +
                              $"Оплачен: {(isPaid == false ? "Нет" : "Да")}",
                        replyMarkup: replyMarkup,
                        cancellationToken: cancellationToken
                        );
            }
        }
    }
}
