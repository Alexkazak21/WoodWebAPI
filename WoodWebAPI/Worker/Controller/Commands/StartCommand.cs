using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using WoodWebAPI.Data.Models.Customer;

namespace WoodWebAPI.Worker.Controller.Commands;

public class StartCommand(IWorkerCreds workerCreds) : ICommand
{
    private readonly IWorkerCreds _workerCreds = workerCreds;
    private readonly ILogger<StartCommand> _logger;
    public TelegramBotClient Client => TelegramWorker.API;

    public string Name => "/start";

    public async Task Execute(Update update, CancellationToken cancellationToken)
    {
        //_logger.LogInformation("Start command");
        if (!cancellationToken.IsCancellationRequested)
        {
            long chatId = -1;
            var messageId = -1;

            if (update.Type == UpdateType.Message)
            {
                chatId = update.Message.From.Id;
                var userFirstName = update.Message.From.FirstName;
                await SendButtonsAsync(chatId, userFirstName: userFirstName);
            }
            else if (update.Type == UpdateType.CallbackQuery)
            {
                chatId = update.CallbackQuery.From.Id;
                messageId = update.CallbackQuery.Message.MessageId;
                var userFirstName = update.CallbackQuery.From.FirstName;
                await SendButtonsAsync(chatId, messageId, userFirstName);
            }
        }
        else
        {
            return;
        }
    }

    private async Task SendButtonsAsync(long chatId, int messageId = -1, string userFirstName = null, CancellationToken cancellationToken = default)
    {
        var userExist = false;
        using (HttpClient client = new HttpClient())
        {
            HttpResponseMessage response = await client.PostAsync($"{_workerCreds.BaseURL}/api/Customer/GetCustomers", new StringContent(""));

            if (response.IsSuccessStatusCode)
            {
                string responseJsonContent = await response.Content.ReadAsStringAsync();
                GetCustomerModel[] customers = JsonConvert.DeserializeObject<GetCustomerModel[]>(responseJsonContent);

                foreach (var customer in customers)
                {
                    try
                    {
                        if (customer.TelegramId == chatId)
                        {
                            userExist = true;
                        }
                    }
                    catch (FormatException)
                    {
                        _logger.LogWarning("Startup command\n" +
                            "\tНевозможно распарсить идентификатор, скорее всего он не равен типу long");
                    }

                }
            }
        }

        var keyboardUserExist = new InlineKeyboardMarkup(
            new[]
            {
                InlineKeyboardButton.WithCallbackData("Вход","/login"),
            }
            );

        var keyboardUserNotExist = new InlineKeyboardMarkup(
            new[]
            {
                InlineKeyboardButton.WithCallbackData("Регистрация", "/signUp"),
            });

        if (messageId == -1)
        {
            if (userExist)
            {
                await Client.SendTextMessageAsync(
                chatId: chatId,
                    text: $"Привет! {userFirstName}" +
                    "\nДля продолжения, войдите в систему",
                    replyMarkup: keyboardUserExist,
                    cancellationToken: cancellationToken);
            }
            else
            {
                await Client.SendTextMessageAsync(
                    chatId: chatId,
                    text: $"Привет! {userFirstName}" +
                    "\nДля продолжения, зарегистрируйтесь",
                    replyMarkup: keyboardUserNotExist,
                    cancellationToken: cancellationToken);
            }
        }
        else
        {
            if (userExist)
            {
                await Client.EditMessageTextAsync(
                chatId: chatId,
                    text: $"Привет! {userFirstName}" +
                    "\nДля продолжения, войдите в систему",
                    messageId: messageId,
                    replyMarkup: keyboardUserExist,
                    cancellationToken: cancellationToken);
            }
            else
            {
                await Client.EditMessageTextAsync(
                    chatId: chatId,
                    text: $"Привет! {userFirstName}" +
                    "\nДля продолжения, зарегистрируйтесь",
                    messageId: messageId,
                    replyMarkup: keyboardUserNotExist,
                    cancellationToken: cancellationToken);
            }
        }


    }

}
