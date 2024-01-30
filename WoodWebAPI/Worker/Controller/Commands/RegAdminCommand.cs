using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using WoodWebAPI.Auth;

namespace WoodWebAPI.Worker.Controller.Commands
{
    public class RegAdminCommand : ICommand
    {
        public TelegramBotClient Client => TelegramWorker.API;

        public string Name => "/reg_admin";

        public async Task Execute(Update update, CancellationToken cancellationToken)
        {
            if(cancellationToken.IsCancellationRequested) return;

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
                    var pass = comandParts[1];
                    using HttpClient httpClient = new HttpClient();
                    var registerAdminCred = new RegisterModel()
                                                {
                                                    Password = pass,
                                                    TelegramID = chatId.ToString(),
                                                };

                    var content = JsonContent.Create(registerAdminCred);
                    var request = await httpClient.PostAsync($"{TelegramWorker.BaseUrl}/api/Authenticate/register-admin", content);
                    var response = JsonConvert.DeserializeObject<Response>( await request.Content.ReadAsStringAsync());
                    
                    if(response.Status == "Success")
                    {

                        await Client.SendTextMessageAsync(
                                chatId: chatId,
                                text: $"{response.Message}",
                                replyMarkup: new InlineKeyboardMarkup(
                                    inlineKeyboardButton: InlineKeyboardButton.WithCallbackData("Главное меню", "/main")),
                                cancellationToken: cancellationToken);
                    }
                    else
                    {
                        await Client.SendTextMessageAsync(
                            chatId: chatId,
                            text: $"{response.Message}",
                            cancellationToken: cancellationToken);
                    }
                }
                else
                {
                    await Client.SendTextMessageAsync(
                        chatId: chatId,
                        text: "Введите пароль ОТВЕТОМ на это сообщение",                       
                        cancellationToken: cancellationToken);
                }
            }         

        }
    }
}
