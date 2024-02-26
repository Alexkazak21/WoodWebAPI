using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using WoodWebAPI.Auth;

namespace WoodWebAPI.Worker.Controller.Commands
{
    public class RegAdminCommand(IWorkerCreds workerCreds) : ICommand
    {
        private readonly IWorkerCreds _workerCreds = workerCreds;
        public TelegramBotClient Client => TelegramWorker.API;

        public string Name => "/reg_admin";

        public async Task Execute(Update update, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested) return;

            long? chatId = null;

            if (update != null)
            {
                if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message)
                {
                    chatId = update.Message.From.Id;
                }
                else if (update.Type == Telegram.Bot.Types.Enums.UpdateType.CallbackQuery)
                {
                    chatId = update.CallbackQuery.From.Id;
                }

                var comandParts = update.Message.Text.Split(":");

                if (comandParts.Length > 1)
                {
                    using HttpClient httpClient = new HttpClient();
                    var registerAdminCred = new RegisterModel()
                    {
                        TelegramID = chatId.ToString(),
                    };

                    var content = JsonContent.Create(registerAdminCred);
                    var requestAuth = await httpClient.PostAsync($"{_workerCreds.BaseURL}/api/Authenticate/register-admin", content);
                    var responseAuth = JsonConvert.DeserializeObject<Response>(await requestAuth.Content.ReadAsStringAsync());
                    

                    if (responseAuth.Status == "Success")
                    {
                        
                        await Client.SendTextMessageAsync(
                                chatId: chatId,
                                text: $"{responseAuth.Message}",
                                replyMarkup: new InlineKeyboardMarkup(
                                    inlineKeyboardButton: InlineKeyboardButton.WithCallbackData("Главное меню", "/main")),
                                cancellationToken: cancellationToken);
                    }
                    else
                    {
                        await Client.SendTextMessageAsync(
                            chatId: chatId,
                            text: $"{responseAuth.Message}",
                            replyMarkup: new InlineKeyboardMarkup(
                                    inlineKeyboardButton: InlineKeyboardButton.WithCallbackData("Главное меню", "/main")),
                            cancellationToken: cancellationToken);
                    }
                }
                else
                {

                }
            }

        }
    }
}
