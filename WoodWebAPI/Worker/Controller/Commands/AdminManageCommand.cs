using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using WoodWebAPI.Data.Models.Customer;

namespace WoodWebAPI.Worker.Controller.Commands;

public class AdminManageCommand : ICommand
{
    public TelegramBotClient Client => TelegramWorker.API;

    public string Name => "/admin_manage";

    public async Task Execute(Update update, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested) return;

        if (update != null)
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
                                        InlineKeyboardButton.WithCallbackData("Добавить",$"/reg_admin:{availableToAddAdmins[0].ChatID}"),
                                    },
                                    new[]
                                    {
                                        InlineKeyboardButton.WithCallbackData("Назад","/admin_manage"),
                                    }
                                });

                            await Client.EditMessageTextAsync(
                                chatId: chatId,
                                messageId: messageId,
                                text: $"Пользователь {availableToAddAdmins[0].ChatID}" +
                                $"\nС именем: {availableToAddAdmins[0].CustomerName}",
                                replyMarkup: replymarkup,
                                cancellationToken: cancellationToken);
                        }
                    }
                    else if ( commandParts[2] != null )
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
                                        InlineKeyboardButton.WithCallbackData("Удалить",$"/del_admin:{availableToDeleteAdmins[0].ChatID}"),
                                    },
                                    new[]
                                    {
                                        InlineKeyboardButton.WithCallbackData("Назад","/admin_manage"),
                                    }
                                });

                            await Client.EditMessageTextAsync(
                                chatId: chatId,
                                messageId: messageId,
                                text: $"Администратор {availableToDeleteAdmins[0].ChatID}" +
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
                var mainAdmin = TelegramWorker.AdminList.FirstOrDefault(x => x.Id == 0);
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
    }

    private async Task<List<GetCustomerAdmin>> GetAvailableCustomers(bool availableCustomer = false)
    {
        List<GetCustomerAdmin> availableCustomersToAdmin = new();

        using HttpClient httpClient = new HttpClient();
        var request = await httpClient.PostAsync($"{TelegramWorker.BaseUrl}/api/Customer/GetCustomerByAdmin", new StringContent(""));
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
                    if (TelegramWorker.AdminList.FirstOrDefault(x => x.TelegramId == customer.ChatID) == null)
                    {
                        availableCustomersToAdmin.Add(customer);
                    }
                }
                else
                {
                    if (TelegramWorker.AdminList.FirstOrDefault(x => x.TelegramId == customer.ChatID && x.Id > 0) != null)
                    {
                        availableCustomersToAdmin.Add(customer);
                    }
                }
                
            }
        }

        return availableCustomersToAdmin;
    }
}
