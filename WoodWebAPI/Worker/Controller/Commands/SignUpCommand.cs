using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Newtonsoft.Json;
using WoodWebAPI.Data.Models.Customer;
using WoodWebAPI.Auth;
using Telegram.Bot.Types.ReplyMarkups;

namespace WoodWebAPI.Worker.Controller.Commands
{
    public class SignUpCommand : ICommand
    {
        public TelegramBotClient Client => TelegramWorker.API;

        public CommandExecutor Executor { get; }

        public string Name => "signUp";

        private string? _password = null;

        public async Task Execute(Update update, CancellationToken cancellationToken)
        {

            if (!cancellationToken.IsCancellationRequested)
            {
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
                //await Client.SendTextMessageAsync(
                //    chatId: chatId,
                //    text :"Введите пароль! ( Ответом на это сообщение)");

                using (HttpClient httpClient = new HttpClient())
                {
                    CreateCustomerDTO customerDTO = new CreateCustomerDTO()
                    {
                        Name = new string((update.CallbackQuery.From.FirstName ?? "anonymous") + (" " + update.CallbackQuery.From.LastName ?? $" {Guid.NewGuid()}")),
                        TelegtamId = chatId.ToString(),
                    };

                    var contentCustomer = JsonContent.Create(customerDTO);

                    //RegisterModel registerModel = new RegisterModel()
                    //{
                    //    TelegramID = chatId.ToString(),
                    //    Password = _password,
                    //};

                    //var contentUser = JsonContent.Create(registerModel);

                    var resultCustomer = await httpClient.PostAsync($"{TelegramWorker.BaseUrl}/api/Customer/CreateCustomers", contentCustomer);
                    //var resultUser = await httpClient.PostAsJsonAsync("http://localhost:5550/api/Authenticate/register", contentUser);

                    if (resultCustomer.IsSuccessStatusCode) //&& resultUser.IsSuccessStatusCode)
                    {

                        WebAppInfo webAppInfo = new WebAppInfo();

                        webAppInfo.Url = "https://woodcutters.mydurable.com/";
                        var inlineMarkup = new InlineKeyboardMarkup(new[] 
                        { 
                            InlineKeyboardButton.WithWebApp(
                                text: "Продолжить в приложении",
                                webAppInfo),

                            InlineKeyboardButton.WithCallbackData(
                                text: "Продолжить в боте",
                                callbackData: "/login")                              
                        });
                        
                        await Client.EditMessageTextAsync(
                            chatId: chatId, 
                            text: "Поздравляем с регистарцией!"
                            +"\nПосле регистрации Вам необходимо войти", 
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
                                inlineKeyboardButton: InlineKeyboardButton.WithCallbackData("В начало","/start")),
                            messageId: messageId,
                            cancellationToken: cancellationToken
                            );
                    }
                }
            }
        }
    }
}
