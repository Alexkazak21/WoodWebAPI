using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using WoodWebAPI.Auth;

namespace WoodWebAPI.Worker.Commands
{
    public class ChangeAdminRoleCommand(IWorkerCreds workerCreds) : ICommand
    {
        private readonly IWorkerCreds _workerCreds = workerCreds;
        public TelegramBotClient Client => TelegramWorker.API;

        public string Name => "/change_role";

        public async Task Execute(Update update, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested || update == null) return;
            try
            {
                long chatId;
                if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message)
                {
                    chatId = update.Message.From.Id;
                }
                else if (update.Type == Telegram.Bot.Types.Enums.UpdateType.CallbackQuery)
                {
                    chatId = update.CallbackQuery.From.Id;
                }
                else
                {
                    chatId = 0L;
                }

                var comandParts = update.CallbackQuery.Data.Split(":");

                if (comandParts.Length > 2 && comandParts[2] == "Admin")
                {

                    await ChangeRole(update, chatId, "Admin", cancellationToken);
                }
                else if(comandParts.Length > 2 && comandParts[2] == "User")
                {
                    await ChangeRole(update, chatId, "User", cancellationToken);
                }
            }
            catch (Exception)
            {
                TelegramWorker.Logger.LogInformation("Проблема в ChangeAdminRoleCommand");
            }
        }

        private async Task ChangeRole(Update update, long chatId,string newRole,CancellationToken cancellationToken)
        {
            try
            {
                using HttpClient httpClient = new HttpClient();
                var changeRoleDto = new ChangeRoleDTO()
                {
                    TelegramId = update.CallbackQuery.Data.Split(':')[1],
                    NewRole = newRole,
                };

                var token = await new CommonChecks(workerCreds).GetToken(chatId);

                var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"{_workerCreds.BaseURL}/api/Authenticate/ChangeRole");
                requestMessage.Headers.Add("Authorization", $"Bearer {token}");
                requestMessage.Content = JsonContent.Create(changeRoleDto);       

                var requestAuth = await httpClient.SendAsync(requestMessage, cancellationToken);
                var res = await requestAuth.Content.ReadAsStringAsync(cancellationToken);
                var responseAuth = JsonConvert.DeserializeObject<Response>(res);

                if (responseAuth.Status == "Success")
                {
                    await Client.EditMessageTextAsync(
                            chatId: chatId,
                            messageId: update.CallbackQuery.Message.MessageId,
                            text: $"{responseAuth.Message}",
                            replyMarkup: new InlineKeyboardMarkup(
                                inlineKeyboardButton: InlineKeyboardButton.WithCallbackData("Главное меню", "/main")),
                            cancellationToken: cancellationToken) ;
                }
                else
                {
                    await Client.EditMessageTextAsync(
                        chatId: chatId,
                        messageId: update.CallbackQuery.Message.MessageId,
                        text: $"{responseAuth.Message}",
                        replyMarkup: new InlineKeyboardMarkup(
                                inlineKeyboardButton: InlineKeyboardButton.WithCallbackData("Главное меню", "/main")),
                        cancellationToken: cancellationToken);
                }
            }
            catch (HttpRequestException) 
            {
                TelegramWorker.Logger.LogError("Ошибка при смене роли, проветьте правильность вводимых данных");
            }
            catch (ApiRequestException)
            {
                TelegramWorker.Logger.LogWarning("Ошибка при отправке сообщения в телеграм бот");
            }
        }
    }
}