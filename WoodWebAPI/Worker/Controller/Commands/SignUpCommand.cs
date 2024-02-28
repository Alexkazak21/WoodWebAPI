using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Newtonsoft.Json;
using WoodWebAPI.Data.Models.Customer;
using WoodWebAPI.Auth;
using Telegram.Bot.Types.ReplyMarkups;

namespace WoodWebAPI.Worker.Controller.Commands
{
    public class SignUpCommand(IWorkerCreds workerCreds) : ICommand
    {
        private readonly IWorkerCreds _workerCreds = workerCreds;
        public TelegramBotClient Client => TelegramWorker.API;

        public CommandExecutor Executor { get; }

        public string Name => "/signUp";

        public async Task Execute(Update update, CancellationToken cancellationToken)
        {

            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            var messageId = 0;

            if (update.Type == UpdateType.CallbackQuery)
            {
                messageId = update.CallbackQuery.Message.MessageId;
            }
            else if (update.Type == UpdateType.Message)
            {
                messageId = update.Message.MessageId;
            }

            var chatId = update.CallbackQuery.From.Id;

            using HttpClient httpClient = new();

            CreateCustomerDTO customerDTO = new()
            {
                Name = new string((update.CallbackQuery.From.FirstName ?? "anonymous") + (" " + update.CallbackQuery.From.LastName ?? $" {Guid.NewGuid()}")),
                TelegtamId = chatId,
                Username = update.CallbackQuery.From.Username ?? $" {Guid.NewGuid()}",
            };

            RegisterModel regCustomerDTO = new()
            {
                TelegramID = chatId.ToString(),
            };

            var contentCustomer = JsonContent.Create(customerDTO);
            var contentRegCustomer = JsonContent.Create(regCustomerDTO);

            var resultCustomer = await httpClient.PostAsync($"{_workerCreds.BaseURL}/api/Customer/CreateCustomers", contentCustomer, cancellationToken: cancellationToken);
            var resultRegCustomer = await httpClient.PostAsJsonAsync($"{_workerCreds.BaseURL}/api/Authenticate/Register", contentRegCustomer, cancellationToken: cancellationToken);

            if (resultCustomer.IsSuccessStatusCode && resultRegCustomer.IsSuccessStatusCode)
            {
                WebAppInfo webAppInfo = new()
                {
                    Url = "https://woodcutters.mydurable.com/"
                };

                var inlineMarkup = new InlineKeyboardMarkup(new[]
                {
                            InlineKeyboardButton.WithWebApp(
                                text: "О нас",
                                webAppInfo),

                            InlineKeyboardButton.WithCallbackData(
                                text: "Продолжить в боте",
                                callbackData: "/main")
                });

                await Client.EditMessageTextAsync(
                    chatId: chatId,
                    text: "Поздравляем с регистарцией!"
                    + "\nПосле регистрации Выберите действие",
                    messageId: messageId,
                    replyMarkup: inlineMarkup,
                    cancellationToken: cancellationToken
                    );
            }
            else
            {
                await Client.EditMessageTextAsync(
                    chatId: chatId,
                    text: "ОШИБКА В РЕГИСТРАЦИИ",
                    replyMarkup: new InlineKeyboardMarkup(
                        inlineKeyboardButton: InlineKeyboardButton.WithCallbackData("В начало", "/start")),
                    messageId: messageId,
                    cancellationToken: cancellationToken
                    );
            }
        }
    }
}