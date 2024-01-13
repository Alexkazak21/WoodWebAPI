using Telegram.Bot;
using Telegram.Bot.Types;
using Newtonsoft.Json;
using WoodWebAPI.Data.Models.Customer;
using WoodWebAPI.Auth;

namespace WoodWebAPI.Worker.Controller.Commands
{
    public class SignUpCommand : ICommand, IListener
    {
        public TelegramBotClient Client => TelegramWorker.API;

        public CommandExecutor Executor { get; }

        public string Name => "signUp";

        private string? _password = null;

        public async Task Execute(Update update, CancellationToken cancellationToken)
        {

            if (!cancellationToken.IsCancellationRequested)
            {
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

                    var resultCustomer = await httpClient.PostAsync("http://localhost:5550/api/Customer/CreateCustomers", contentCustomer);
                    //var resultUser = await httpClient.PostAsJsonAsync("http://localhost:5550/api/Authenticate/register", contentUser);

                    if (resultCustomer.IsSuccessStatusCode) //&& resultUser.IsSuccessStatusCode)
                    {
                        await Client.SendTextMessageAsync(chatId, "Поздравляем с регистарцией!");
                    }
                    else
                    {
                        await Client.SendTextMessageAsync(chatId, "ОШИБКА В РЕГИСТРАЦИИ");
                    }
                }
            }
        }

        public SignUpCommand(CommandExecutor executor)
        {
            Executor = executor;
        }

        public async Task GetUpdate(Update update,CancellationToken cancellationToken)
        {
            long chatId = update.Message.Chat.Id;
            if (update.Message.Text == null) //Проверочка
                return;

            if (_password == null) //Получаем пароль пользователя
            {
                _password = update.Message.Text;

                using (HttpClient httpClient = new HttpClient())
                {
                    CreateCustomerDTO customerDTO = new CreateCustomerDTO()
                    {
                        Name = new string((update.CallbackQuery.From.FirstName ?? "anonymous") + (" " + update.CallbackQuery.From.LastName ?? $" {Guid.NewGuid()}")),
                        TelegtamId = chatId.ToString(),
                    };

                    var contentCustomer = JsonContent.Create(customerDTO);

                    RegisterModel registerModel = new RegisterModel()
                    {
                        TelegramID = chatId.ToString(),
                        Password = _password,
                    };

                    var contentUser = JsonContent.Create(registerModel);

                    var resultCustomer = await httpClient.PostAsJsonAsync("http://localhost:5550/api/Customer/CreateCustomers", contentCustomer);
                    var resultUser = await httpClient.PostAsJsonAsync("http://localhost:5550/api/Authenticate/register", contentUser);

                    if (resultCustomer.IsSuccessStatusCode && resultUser.IsSuccessStatusCode)
                    {
                        await Client.SendTextMessageAsync(chatId, "Поздравляем с регистарцией!");
                    }
                    else
                    {
                        await Client.SendTextMessageAsync(chatId, "ОШИБКА В РЕГИСТРАЦИИ");
                    }

                }
                Executor.StopListen();
            }
        }
    }
}
