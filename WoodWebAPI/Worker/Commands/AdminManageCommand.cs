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
            string[]? commandParts = null;

            if (update.Type == Telegram.Bot.Types.Enums.UpdateType.CallbackQuery)
            {
                chatId = update.CallbackQuery.From.Id;
                messageId = update.CallbackQuery.Message.MessageId;
                commandParts = update.CallbackQuery.Data.Split(":");
            }

            if (commandParts != null && commandParts.Length > 1)
            {
                if (commandParts[1] == "add")
                {
                    List<GetCustomerAdmin> availableToAddAdmins = new();

                    if (commandParts.Length < 3)
                    {
                        availableToAddAdmins = await GetAvailableCustomers(true);

                        if (availableToAddAdmins.Count == 1)
                        {
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

                            await Client.EditMessageTextAsync(
                                chatId: chatId,
                                messageId: messageId,
                                text: $"Пользователь {availableToAddAdmins[0].TelegramId}" +
                                $"\nС именем: {availableToAddAdmins[0].CustomerName}",
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
                    else if (commandParts[2] != null)
                    {

                    }
                }
                else if (commandParts[1] == "delete")
                {
                    List<GetCustomerAdmin> availableToDeleteAdmins = new();

                    if (commandParts.Length < 3)
                    {
                        availableToDeleteAdmins = await GetAvailableCustomers();

                        if (availableToDeleteAdmins.Count == 1)
                        {
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

                            await Client.EditMessageTextAsync(
                                chatId: chatId,
                                messageId: messageId,
                                text: $"Администратор {availableToDeleteAdmins[0].TelegramId}" +
                                $"\nС именем: {availableToDeleteAdmins[0].CustomerName}",
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
                    else if (commandParts[2] != null)
                    {

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
