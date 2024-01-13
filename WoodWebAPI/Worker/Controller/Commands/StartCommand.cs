using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using WoodWebAPI.Data.Models.Customer;

namespace WoodWebAPI.Worker.Controller.Commands;

public class StartCommand : ICommand
{
    public TelegramBotClient Client => TelegramWorker.API;

    public string Name => "/start";

    public async Task Execute(Update update, CancellationToken cancellationToken)
    {
        if (!cancellationToken.IsCancellationRequested)
        {
            long chatId = update.Message.Chat.Id;
            await Client.SendTextMessageAsync(chatId, "Привет! " + update.Message.Chat.FirstName);

            var userExist = false;
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.PostAsync("http://localhost:5550/api/Customer/GetCustomers", new StringContent(""));

                if (response.IsSuccessStatusCode)
                {
                    string responseJsonContent = await response.Content.ReadAsStringAsync();
                    GetCustomerModel[] customers = JsonConvert.DeserializeObject<GetCustomerModel[]>(responseJsonContent);

                    foreach (var customer in customers)
                    {
                        try
                        {
                            if (long.Parse(customer.TelegramId) == chatId)
                            {
                                userExist = true;
                            }
                        }
                        catch (FormatException ex)
                        {
                            TelegramWorker.Logger
                                 .LogWarning("Startup command\n" + 
                                "\tНевозможно распарсить идентификатор, скорее всего он не равен типу long");
                        }

                    }
                }
            }

            await SendButtonsAsync(update.Message.Chat.Id, userExist);

        }
        else
        {
            return;
        }
    }

    private async Task SendButtonsAsync(long chatId, bool userExist)
    {
        var keyboardUserExist = new InlineKeyboardMarkup(
            new[]
            {
                InlineKeyboardButton.WithCallbackData("Вход","login"),
            }
            );

        var keyboardUserNotExist = new InlineKeyboardMarkup(
            new[]
            {
                InlineKeyboardButton.WithCallbackData("Регистрация", "signUp"),
            });

        Message message = new Message();
        if (userExist)
        {
            message = await Client.SendTextMessageAsync(chatId, "Для продолжения, войдите в систему", replyMarkup: keyboardUserExist);
        }
        else
        {
            message = await Client.SendTextMessageAsync(chatId, "Для продолжения, зарегистрируйтесь", replyMarkup: keyboardUserNotExist);
        }

    }

}
