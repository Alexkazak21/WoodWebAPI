using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using WoodWebAPI.Data;
using WoodWebAPI.Data.Models.Customer;

namespace WoodWebAPI.Worker.Commands;

public class AdminManageCommand(IWorkerCreds workerCreds,WoodDBContext wood) : ICommand
{
    private readonly IWorkerCreds _workerCreds = workerCreds;
    private readonly WoodDBContext _dbContext = wood;
    public TelegramBotClient Client => TelegramWorker.API;

    public string Name => "/admin_manage";

    public async Task Execute(Update update, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested || update == null) return;

        try
        {
            var chatId = 0l;
            var messageId = 0;
            List<string>? commandParts = new();

            if (update.Type == Telegram.Bot.Types.Enums.UpdateType.CallbackQuery)
            {
                chatId = update.CallbackQuery.From.Id;
                messageId = update.CallbackQuery.Message.MessageId;
                commandParts.AddRange(update.CallbackQuery.Data.Split(":"));
            }

            if (commandParts != null && commandParts.Count > 1)
            {
                if (commandParts[1] == "add")
                {
                    List<GetCustomerAdmin> availableToAddAdmins = new();

                    if (commandParts.Count <= 3)
                    {
                        availableToAddAdmins = await GetAvailableCustomers(true);

                        if (availableToAddAdmins.Count > 0)
                        {
                            long telegramId = 0L;
                            if(commandParts.Count == 2)
                            {
                                commandParts.Add(availableToAddAdmins[0].TelegramId.ToString());
                                telegramId = availableToAddAdmins[0].TelegramId;
                            }
                            else
                            {
                                telegramId = long.Parse(commandParts[2]);
                            }

                            var replymarkup = new InlineKeyboardMarkup(
                                new[]
                                {
                                    new[]
                                    {
                                        InlineKeyboardButton.WithCallbackData("Добавить",$"/change_role:{availableToAddAdmins[0].TelegramId}:Admin"),
                                    },
                                    new[]
                                    {
                                        InlineKeyboardButton.WithCallbackData("Назад","/admin_manage"),
                                    }
                                });

                            if (availableToAddAdmins[0].TelegramId == telegramId && availableToAddAdmins.Count > 1)
                            {
                                replymarkup = new InlineKeyboardMarkup(
                                new[]
                                {
                                    new[]
                                    {
                                        InlineKeyboardButton.WithCallbackData(">",$"/admin_manage:add:{availableToAddAdmins[1].TelegramId}")
                                    },
                                    new[]
                                    {
                                        InlineKeyboardButton.WithCallbackData("Добавить",$"/change_role:{availableToAddAdmins[0].TelegramId}:Admin"),
                                    },
                                    new[]
                                    {
                                        InlineKeyboardButton.WithCallbackData("Назад","/admin_manage"),
                                    }
                                });
                            }
                            else if (availableToAddAdmins[^1].TelegramId == telegramId && availableToAddAdmins.Count > 1)
                            {
                                replymarkup = new InlineKeyboardMarkup(
                                new[]
                                {
                                    new[]
                                    {
                                        InlineKeyboardButton.WithCallbackData("<",$"/admin_manage:add:{availableToAddAdmins[^2].TelegramId}")
                                    },
                                    new[]
                                    {
                                        InlineKeyboardButton.WithCallbackData("Добавить",$"/change_role:{availableToAddAdmins[^1].TelegramId}:Admin"),
                                    },
                                    new[]
                                    {
                                        InlineKeyboardButton.WithCallbackData("Назад","/admin_manage"),
                                    }
                                });
                            }
                            else if (availableToAddAdmins.Count > 1)
                            {
                                replymarkup = new InlineKeyboardMarkup(
                                new[]
                                {
                                    new[]
                                    {
                                        InlineKeyboardButton.WithCallbackData("<",$"/admin_manage:add:{availableToAddAdmins[availableToAddAdmins.FindIndex(x => x.TelegramId == telegramId) - 1].TelegramId}"),
                                        InlineKeyboardButton.WithCallbackData(">",$"/admin_manage:add:{availableToAddAdmins[availableToAddAdmins.FindIndex(x => x.TelegramId == telegramId) + 1].TelegramId}")
                                    },
                                    new[]
                                    {
                                        InlineKeyboardButton.WithCallbackData("Добавить",$"/change_role:{availableToAddAdmins[availableToAddAdmins.FindIndex(x => x.TelegramId == telegramId)].TelegramId}:Admin"),
                                    },
                                    new[]
                                    {
                                        InlineKeyboardButton.WithCallbackData("Назад","/admin_manage"),
                                    }
                                });
                            }

                            await Client.EditMessageTextAsync(
                                chatId: chatId,
                                messageId: messageId,
                                text: $"Пользователь {availableToAddAdmins[availableToAddAdmins.FindIndex(x => x.TelegramId == telegramId)].TelegramId}" +
                                $"\nС именем: {availableToAddAdmins[availableToAddAdmins.FindIndex(x => x.TelegramId == telegramId)].CustomerName}",
                                replyMarkup: replymarkup,
                                cancellationToken: cancellationToken);
                        }
                        else if (availableToAddAdmins.Count == 0)
                        {
                            await Client.EditMessageTextAsync(
                                chatId: chatId,
                                messageId: messageId,
                                text: "Нет пользователей для добавления",
                                replyMarkup: new InlineKeyboardMarkup(
                                   [
                                       InlineKeyboardButton.WithCallbackData("На главную","/main")
                                    ]),

                                cancellationToken: cancellationToken);
                        }
                    }
                }
                else if (commandParts[1] == "delete")
                {
                    List<GetCustomerAdmin> availableToDeleteAdmins = new();

                    if (commandParts.Count <= 3)
                    {
                        availableToDeleteAdmins = await GetAvailableCustomers();

                        if (availableToDeleteAdmins.Count > 0)
                        {
                            long telegramId = 0L;
                            if (commandParts.Count == 2)
                            {
                                commandParts.Add(availableToDeleteAdmins[0].TelegramId.ToString());
                                telegramId = availableToDeleteAdmins[0].TelegramId;
                            }
                            else
                            {
                                telegramId = long.Parse(commandParts[2]);
                            }

                            var replymarkup = new InlineKeyboardMarkup(
                                new[]
                                {
                                    new[]
                                    {
                                        InlineKeyboardButton.WithCallbackData("Удалить",$"/change_role:{availableToDeleteAdmins[0].TelegramId}:User"),
                                    },
                                    new[]
                                    {
                                        InlineKeyboardButton.WithCallbackData("Назад","/admin_manage"),
                                    }
                                });

                            if (availableToDeleteAdmins[0].TelegramId == telegramId && availableToDeleteAdmins.Count > 1)
                            {
                                replymarkup = new InlineKeyboardMarkup(
                                new[]
                                {
                                    new[]
                                    {
                                        InlineKeyboardButton.WithCallbackData(">",$"/admin_manage:delete:{availableToDeleteAdmins[1].TelegramId}")
                                    },
                                    new[]
                                    {
                                        InlineKeyboardButton.WithCallbackData("Удалить",$"/change_role:{availableToDeleteAdmins[0].TelegramId}:User"),
                                    },
                                    new[]
                                    {
                                        InlineKeyboardButton.WithCallbackData("Назад","/admin_manage"),
                                    }
                                });
                            }
                            else if (availableToDeleteAdmins[^1].TelegramId == telegramId && availableToDeleteAdmins.Count > 1)
                            {
                                replymarkup = new InlineKeyboardMarkup(
                                new[]
                                {
                                    new[]
                                    {
                                        InlineKeyboardButton.WithCallbackData("<",$"/admin_manage:delete:{availableToDeleteAdmins[^2].TelegramId}")
                                    },
                                    new[]
                                    {
                                        InlineKeyboardButton.WithCallbackData("Удалить",$"/change_role:{availableToDeleteAdmins[^1].TelegramId}:User"),
                                    },
                                    new[]
                                    {
                                        InlineKeyboardButton.WithCallbackData("Назад","/admin_manage"),
                                    }
                                });
                            }
                            else if (availableToDeleteAdmins.Count > 1)
                            {
                                replymarkup = new InlineKeyboardMarkup(
                                new[]
                                {
                                    new[]
                                    {
                                        InlineKeyboardButton.WithCallbackData("<",$"/admin_manage:delete:{availableToDeleteAdmins[availableToDeleteAdmins.FindIndex(x => x.TelegramId == telegramId) - 1].TelegramId}"),
                                        InlineKeyboardButton.WithCallbackData(">",$"/admin_manage:delete:{availableToDeleteAdmins[availableToDeleteAdmins.FindIndex(x => x.TelegramId == telegramId) + 1].TelegramId}")
                                    },
                                    new[]
                                    {
                                        InlineKeyboardButton.WithCallbackData("Удалить",$"/change_role:{availableToDeleteAdmins[availableToDeleteAdmins.FindIndex(x => x.TelegramId == telegramId)].TelegramId}:User"),
                                    },
                                    new[]
                                    {
                                        InlineKeyboardButton.WithCallbackData("Назад","/admin_manage"),
                                    }
                                });
                            }
                            await Client.EditMessageTextAsync(
                                chatId: chatId,
                                messageId: messageId,
                                text: $"Администратор {availableToDeleteAdmins[availableToDeleteAdmins.FindIndex(x => x.TelegramId == telegramId)].TelegramId}" +
                                $"\nС именем: {availableToDeleteAdmins[availableToDeleteAdmins.FindIndex(x => x.TelegramId == telegramId)].CustomerName}",
                                replyMarkup: replymarkup,
                                cancellationToken: cancellationToken);
                        }
                        else if (availableToDeleteAdmins.Count == 0)
                        {
                            var replymarkup = new InlineKeyboardMarkup(
                                new[]
                                {
                                    new[]
                                    {
                                        InlineKeyboardButton.WithCallbackData("Назад","/admin_manage"),
                                    }
                                });

                            await Client.EditMessageTextAsync(
                                chatId: chatId,
                                messageId: messageId,
                                text: "Нет администраторов для удаления",
                                replyMarkup: replymarkup,
                                cancellationToken: cancellationToken);
                        }
                    }
                }
                else
                {
                    TelegramWorker.Logger.LogWarning("Wrong attribute while executing /admin_manage");
                }
            }
            else
            {
                var mainAdmin = _dbContext.IsAdmin.FirstOrDefault(x => x.Id == 1);
                var replyMarkup = new InlineKeyboardMarkup(
                    new[]
                    {
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("Добавить администратора","/admin_manage:add"),
                        },
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("Удалить администратора","/admin_manage:delete"),
                        },
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("На главную","/main"),
                        }
                    });

                await Client.EditMessageTextAsync(
                    chatId: chatId,
                    messageId: messageId,
                    text: "Главный администратор" +
                    $"\nИмя: {mainAdmin.TelegramUsername}",
                    replyMarkup: replyMarkup,
                    cancellationToken: cancellationToken);
            }
        }
        catch (ApiRequestException)
        {
            TelegramWorker.Logger.LogError("Ошибка обновления телеграм");
        }
        catch (Exception)
        {
            TelegramWorker.Logger.LogError("Ошибка in AdminManageCommand");
        }
    }

    private async Task<List<GetCustomerAdmin>> GetAvailableCustomers(bool availableCustomer = false)
    {
        List<GetCustomerAdmin> availableCustomersToAdmin = new();

        using HttpClient httpClient = new HttpClient();
        var request = await httpClient.PostAsync($"{_workerCreds.BaseURL}/api/Customer/GetCustomerByAdmin", new StringContent(""));
        List<GetCustomerAdmin> responce = null;
        if (request.IsSuccessStatusCode)
        {
            responce = JsonConvert.DeserializeObject<GetCustomerAdmin[]?>(await request.Content.ReadAsStringAsync()).ToList();
        }

        if (responce != null)
        {
            foreach (var customer in responce)
            {
                if (availableCustomer)
                {
                    if (_dbContext.IsAdmin.FirstOrDefault(x => x.TelegramId == customer.TelegramId.ToString()) == null)
                    {
                        availableCustomersToAdmin.Add(customer);                        
                    }
                }
                else
                {
                    if (_dbContext.IsAdmin.FirstOrDefault(x => x.TelegramId == customer.TelegramId.ToString() && x.Id > 1) != null)
                    {
                        availableCustomersToAdmin.Add(customer);                        
                    }
                }

            }
        }

        return availableCustomersToAdmin;
    }
}
