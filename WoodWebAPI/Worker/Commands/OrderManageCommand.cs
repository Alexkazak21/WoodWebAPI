using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Linq;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using WoodWebAPI.Data;
using WoodWebAPI.Data.Entities;
using WoodWebAPI.Data.Models;
using WoodWebAPI.Data.Models.Customer;
using WoodWebAPI.Data.Models.Order;
using WoodWebAPI.Services.Extensions;

namespace WoodWebAPI.Worker.Commands;

public class OrderManageCommand(IWorkerCreds workerCreds, WoodDBContext wood) : ICommand
{
    private readonly WoodDBContext _dbContext = wood;
    private readonly IWorkerCreds _workerCreds = workerCreds;
    public TelegramBotClient Client => TelegramWorker.API;

    public string Name => "/order_manage";

    public async Task Execute(Update update, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested || update == null) { return; }

        var chatId = 0L;
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
                    var result = await ChangeOrderStatusAsync(int.Parse(commandParts[2]), OrderStatus.Completed);

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
                var result = await ChangeOrderStatusAsync(int.Parse(commandParts[2]), OrderStatus.Verified);

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
            else if(commandParts.Length > 2 && commandParts[1] == "approve")
            {
                var result = await ChangeOrderStatusAsync(int.Parse(commandParts[2]), OrderStatus.Approved);

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

    private async Task<ExecResultModel> ChangeOrderStatusAsync(int orderId, OrderStatus newStatus)
    {
        try
        {
            var content = JsonContent.Create(new ChangeStatusDTO
            {
                OrderId = orderId,
                NewStatus = newStatus
            });
            ExecResultModel? completeResult = null;
            using HttpClient httpClient = new();

            {
                var request = await httpClient.PostAsync($"{_workerCreds.BaseURL}/api/Order/ChangeStatusOfOrder", content);
                var responce = await request.Content.ReadAsStringAsync();
                completeResult = JsonConvert.DeserializeObject<ExecResultModel>(responce);
            }



            if (completeResult.Success && newStatus == OrderStatus.Completed)
            {
                var responce = await httpClient.PostAsync($"{_workerCreds.BaseURL}/api/Customer/GetCustomers", new StringContent(""));
                var responseJsonContent = await responce.Content.ReadAsStringAsync();
                GetCustomerModel[] customersArray = JsonConvert.DeserializeObject<GetCustomerModel[]>(responseJsonContent);

                foreach (var customer in customersArray)
                {
                    if (customer.Orders.FirstOrDefault(x => x.Id == orderId) != null)
                    {
                        var volume = await new CommonChecks(_workerCreds).GetVolume(customer.TelegramId, orderId);

                        var ammountToPay = decimal.Round(_workerCreds.PriceForM3 * decimal.Parse(volume.ToString()), 2, MidpointRounding.AwayFromZero) > _workerCreds.MinPrice ?
                                            decimal.Round(_workerCreds.PriceForM3 * decimal.Parse(volume.ToString()), 2, MidpointRounding.AwayFromZero) : _workerCreds.MinPrice;
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


            return completeResult;
        }
        catch (Exception)
        {
            return new ExecResultModel
            {
                Success = false,
                Message = "Что-то пошло не так, попробуйте позже"
            };
        }
    }

    private async Task ShowAllOrders(Update update, int? orderId = null, bool? verified = null, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested || update == null) return;


        try
        {
            var customerId = -1l;

            var messageId = update.CallbackQuery.Message.MessageId;

            bool CustomerIsAdmin = false;

            List<OrderModel>? orders = null;

            if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message)
            {
                customerId = update.Message.From.Id;
                CustomerIsAdmin = await new CommonChecks(_workerCreds).CheckCustomer(customerId, cancellationToken);
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

            using HttpClient httpClient = new();

            var content = new StringContent("");

            var responce = await httpClient.PostAsync($"{_workerCreds.BaseURL}/api/Order/GetFullOrdersList", content, cancellationToken);
            var responseJsonContent = await responce.Content.ReadAsStringAsync(cancellationToken);
            orders = [.. JsonConvert.DeserializeObject<OrderModel[]?>(responseJsonContent)];


            if (verified == true)
            {
                orders = orders.Where(x => x.Status == OrderStatus.Verified).ToList();
                await SendTemplateAsync(orders, chatId, messageId, verified: true, orderId: orderId, cancellationToken: cancellationToken);
            }
            else if (verified == false)
            {
                orders = orders.Where(x => x.Status < OrderStatus.Verified).ToList();
                await SendTemplateAsync(orders, chatId, messageId, verified: false, orderId: orderId, cancellationToken: cancellationToken);
            }
            else
            {
                await SendTemplateAsync(orders, chatId, messageId, orderId: orderId, cancellationToken: cancellationToken);
            }
        }
        catch (Exception)
        {
            TelegramWorker.Logger.LogError("Ошибка в ShowAllOrders");
        }        
    }



    private async Task SendTemplateAsync(List<OrderModel>? orders, long chatId, int messageId, bool? verified = null, int? orderId = null, CancellationToken cancellationToken = default)
    {
        try
        {
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
                catch (ApiRequestException ex)
                {
                    TelegramWorker.Logger.LogError(ex.Message);
                }
            }

            InlineKeyboardButton proccessButton = new("");

            //Нет Id заказа
            if (orders != null && orderId == null)
            {
                orderId = orders[0].Id;
                
                if(orders.First(x => x.Id == orderId).Status == OrderStatus.NewOrder)
                {
                    proccessButton = InlineKeyboardButton.WithCallbackData("Подтвердить", $"/order_manage:approve:{orderId}");
                }
                else if (orders.First(x => x.Id == orderId).Status < OrderStatus.Verified)
                {
                    proccessButton = InlineKeyboardButton.WithCallbackData("Принять в работу", $"/order_manage:verify:{orderId}");
                }
                else if (orders.First(x => x.Id == orderId).Status == OrderStatus.Verified)
                {
                    proccessButton = InlineKeyboardButton.WithCallbackData("Завершить заказ", $"/order_manage:complete:{orderId}");
                }
                else if (orders.First(x => x.Id == orderId).Status == OrderStatus.Completed)
                {
                    var volume = await new CommonChecks(_workerCreds).GetVolume(orders[0].CustomerId, orders[0].Id);

                    var ammountToPay = decimal.Round(_workerCreds.PriceForM3 * decimal.Parse(volume.ToString()), 2, MidpointRounding.AwayFromZero) > _workerCreds.MinPrice ?
                                        decimal.Round(_workerCreds.PriceForM3 * decimal.Parse(volume.ToString()), 2, MidpointRounding.AwayFromZero) : _workerCreds.MinPrice;
                    proccessButton = InlineKeyboardButton.WithCallbackData("Запросить оплату", $"/payment:{orders[0].CustomerId}:{ammountToPay}:{orderId}");
                }
                else
                {
                    proccessButton = InlineKeyboardButton.WithCallbackData("В архиве", "skip");
                }
            }
            else
            {
                if (orders.First(x => x.Id == orderId).Status == OrderStatus.NewOrder)
                {
                    proccessButton = InlineKeyboardButton.WithCallbackData("Подтвердить", $"/order_manage:approve:{orderId}");
                }
                else if (orders.First(x => x.Id == orderId).Status < OrderStatus.Verified)
                {
                    proccessButton = InlineKeyboardButton.WithCallbackData("Принять в работу", $"/order_manage:verify:{orders[orders.IndexOf(orders.Where(x => x.Id == orderId).First())].Id}");
                }
                else if (orders.First(x => x.Id == orderId).Status == OrderStatus.Verified)
                {
                    proccessButton = InlineKeyboardButton.WithCallbackData("Завершить заказ", $"/order_manage:complete:{orders[orders.IndexOf(orders.Where(x => x.Id == orderId).First())].Id}");
                }
                else if (orders.First(x => x.Id == orderId).Status == OrderStatus.Completed)
                {
                    var volume = await new CommonChecks(_workerCreds).GetVolume(orders[orders.IndexOf(orders.Where(x => x.Id == orderId).First())].CustomerId, orders[orders.IndexOf(orders.Where(x => x.Id == orderId).First())].Id);

                    var ammountToPay = decimal.Round(_workerCreds.PriceForM3 * decimal.Parse(volume.ToString()), 2, MidpointRounding.AwayFromZero) > _workerCreds.MinPrice ?
                                        decimal.Round(_workerCreds.PriceForM3 * decimal.Parse(volume.ToString()), 2, MidpointRounding.AwayFromZero) : _workerCreds.MinPrice;
                    proccessButton = InlineKeyboardButton.WithCallbackData("Запросить оплату", $"/payment:{orders[orders.IndexOf(orders.Where(x => x.Id == orderId).First())].CustomerId}:{ammountToPay}:{orderId}");
                }
                else
                {
                    proccessButton = InlineKeyboardButton.WithCallbackData("В архиве", "skip");
                }
            }
            string orderStatusMessage = orders.First(x => x.Id == orderId).OrderStatusMessage();

            // отображение целого списк заказов без навигации, со статусом меньше Verified
            if (orders != null && orderId == orders[0].Id && (verified == false || verified == null || verified == true))
            {
                var volumeTotal = await new CommonChecks(_workerCreds).GetVolume(orders.Where(x => x.Id == orders[0].Id).Select(x => x.CustomerId).First(), orders[0].Id);
                if (orders.Count == 1)
                {
                    var customerId = orders.Where(x => x.Id == orderId).First().CustomerId;
                    var userName = _dbContext.Customers.Where(x => x.TelegramID == customerId).First().Name;
                    await Client.EditMessageTextAsync(
                        chatId: chatId,
                        messageId: messageId,
                        text: $"Заказ № {orders[0].Id}\n" +
                        $"Пользователь: {_dbContext.Customers.Where(x => x.TelegramID == orders[0].CustomerId).First().Name}\n" +
                        $"Создан {orders[0].CreatedAt.AddHours(3)}\n" +
                        $"Объёмом: {volumeTotal} м3\n" +
                        $"{orderStatusMessage}",
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
                        $"Пользователь: {_dbContext.Customers.Where(x => x.TelegramID == orders[0].CustomerId).First().Name}\n" +
                        $"Создан {orders[0].CreatedAt.AddHours(3)}\n" +
                        $"Объёмом: {volumeTotal} м3\n" +
                        $"{orderStatusMessage}",
                        replyMarkup: new InlineKeyboardMarkup(
                           new[]
                           {
                               new[]
                               {
                                   InlineKeyboardButton.WithCallbackData(">", $"/order_manage:{(verified == null ? "all" : $"{verified.ToString().ToLower()}")}:{orders[1].Id}"),
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
            else if (orders != null && orderId != null && (verified == false || verified == null))
            {
                var volumeTotal = await new CommonChecks(_workerCreds).GetVolume(orders.Where(x => x.Id == orderId).Select(x => x.CustomerId).First(), (int)orderId);
                InlineKeyboardMarkup replyMarkup = null;
                var customerId = orders.Where(x => x.Id == orderId).First().CustomerId;
                var userName = _dbContext.Customers.Where(x => x.TelegramID == customerId).First().Name; 

                if (orders[0].Id == orderId)
                {
                    replyMarkup = new InlineKeyboardMarkup(
                           new[]
                           {
                               new[]
                               {
                                    InlineKeyboardButton.WithCallbackData(">", $"/order_manage:{(verified == null ? "all" : $"{verified.ToString().ToLower()}")}:{orders[1].Id}"),
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
                                    InlineKeyboardButton.WithCallbackData("<", $"/order_manage:{(verified == null ? "all" : $"{verified.ToString().ToLower()}")}:{orders[^2].Id}"),
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
                                   InlineKeyboardButton.WithCallbackData("<", $"/order_manage:{(verified == null ? "all" : $"{verified.ToString().ToLower()}")}:{orders[orders.IndexOf(orders.Where(x => x.Id == orderId).First()) - 1].Id}"),
                                   InlineKeyboardButton.WithCallbackData(">", $"/order_manage:{(verified == null ? "all" : $"{verified.ToString().ToLower()}")}:{orders[orders.IndexOf(orders.Where(x => x.Id == orderId).First()) + 1].Id}"),
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
                              $"Пользователь: {userName}\n" +
                              $"Создан {orders[orders.IndexOf(orders.Where(x => x.Id == orderId).First())].CreatedAt.AddHours(3)}\n" +
                              $"Объёмом: {volumeTotal} м3\n" +
                              $"{orderStatusMessage}",
                        replyMarkup: replyMarkup,
                        cancellationToken: cancellationToken
                        );
            }

            //отображение списка заказов без навигации, признак Verified = true
            else if (orders != null && orderId == null && verified == true)
            {
                var volumeTotal = await new CommonChecks(_workerCreds).GetVolume(orders[0].CustomerId, orders[0].Id);

                var customerId = orders[0].CustomerId;
                var userName = _dbContext.Customers.Where(x => x.TelegramID == customerId).First().Name;

                if (orders.Count == 1)
                {
                    await Client.EditMessageTextAsync(
                        chatId: chatId,
                        messageId: messageId,
                        text: $"Заказ № {orders[0].Id}\n" +
                        $"Пользователь: {userName}\n" +
                        $"Создан {orders[0].CreatedAt.AddHours(3)}\n" +
                        $"Объёмом: {volumeTotal} м3\n" +
                        $"{orderStatusMessage}",
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
                        rightButton = InlineKeyboardButton.WithCallbackData(">", $"/order_manage:{(verified == null ? "all" : $"{verified.ToString().ToLower()}")}:{orders[1].Id}");
                    }

                    await Client.EditMessageTextAsync(
                        chatId: chatId,
                        messageId: messageId,
                        text: $"Заказ № {orders[0].Id}\n" +
                        $"Пользователь: {userName}\n" +
                        $"Создан {orders[0].CreatedAt.AddHours(3)}\n" +
                        $"Объёмом: {volumeTotal} м3\n" +
                        $"{orderStatusMessage}",
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
                var volumeTotal = await new CommonChecks(_workerCreds).GetVolume(orders.Where(x => x.Id == orderId).Select(x => x.CustomerId).First(), (int)orderId);

                var customerId = orders.Where(x => x.Id == orderId).First().CustomerId;
                var userName = _dbContext.Customers.Where(x => x.TelegramID == customerId).First().Name;

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
                              $"Пользователь: {userName}\n" +
                              $"Создан {orders[orders.IndexOf(orders.Where(x => x.Id == orderId).First())].CreatedAt.AddHours(3)}\n" +
                              $"Объёмом: {volumeTotal} м3\n" +
                              $"{orderStatusMessage}",
                        replyMarkup: replyMarkup,
                        cancellationToken: cancellationToken
                        );
            }
        }
        catch (Exception ex)
        {
            TelegramWorker.Logger.LogWarning($"Ошибка в SendTemplateAsync\n{ex.Message}");
        }
    }
}