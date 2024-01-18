using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using WoodWebAPI.Data.Models.Customer;
using WoodWebAPI.Data.Models.Order;

namespace WoodWebAPI.Worker.Controller.Commands;

public class MainCommand : ICommand
{
    public TelegramBotClient Client => TelegramWorker.API;

    public string Name => "/main";

    public async Task Execute(Update update, CancellationToken cancellationToken)
    {
        if (!cancellationToken.IsCancellationRequested)
        {
            if (update != null)
            {
                var userExist = false;

                long chatid = -1;

                if (update.Type == UpdateType.Message)
                {
                    chatid = update.Message.Chat.Id;

                    userExist = await CheckCustomer(chatid, cancellationToken);

                }
                else if (update.Type == UpdateType.CallbackQuery)
                {
                    chatid = update.CallbackQuery.From.Id;

                    userExist = await CheckCustomer(chatid, cancellationToken);
                }

                if (userExist && chatid != -1)
                {
                    using (HttpClient httpClient = new HttpClient())
                    {
                        var content = JsonContent.Create(
                            new GetOrdersDTO()
                            {
                                Customer_TelegramID = chatid.ToString(),
                            });

                        var responce = await httpClient.PostAsync("http://localhost:5550/api/Order/GetOrdersOfCustomer", content, cancellationToken);
                        var responseJsonContent = await responce.Content.ReadAsStringAsync(cancellationToken);
                        OrderModel[]? orders = JsonConvert.DeserializeObject<OrderModel[]?>(responseJsonContent);

                        if (orders != null)
                        {
                            if (orders.Length == 0)
                            {
                                await Client.SendTextMessageAsync(
                                    chatId: chatid,
                                    text: "У вас пока нет заказов. \nХотите создать заказ?",
                                    replyMarkup: new InlineKeyboardMarkup(
                                                    new[]
                                                    {
                                                    InlineKeyboardButton.WithCallbackData("Создать заказ","/new_order"),
                                                    }),
                                    cancellationToken: cancellationToken);
                            }
                            else if (orders.Length > 5)
                            {
                                await Client.SendTextMessageAsync(
                                    chatId: chatid,
                                    text: "Максимальное колличество одновременных заказов 4",
                                    cancellationToken: cancellationToken);
                            }
                            else if (orders != null && orders.Length < 5)
                            {
                                var keybordButtons = new List<InlineKeyboardButton>();
                                for (int i = 0; i < orders.Length; i++)
                                {
                                    keybordButtons.Add(
                                        InlineKeyboardButton.WithCallbackData($"{orders[i].OrderId}", $"order:{orders[i].Id}"));

                                }

                                InlineKeyboardMarkup? keyboard = null;
                                if (orders.Length < 4)
                                {
                                    keyboard = new InlineKeyboardMarkup(new[]
                                    {
                                        keybordButtons.ToArray<InlineKeyboardButton>(),
                                        [
                                            InlineKeyboardButton.WithCallbackData("Добавить заказ", "/new_order"),
                                        ]
                                    });
                                }
                                else 
                                {
                                    keyboard = new InlineKeyboardMarkup(new[]
                                    {
                                        keybordButtons.ToArray<InlineKeyboardButton>(),
                                        [
                                            InlineKeyboardButton.WithCallbackData("Удалить заказ", "/remove_order"),
                                        ]
                                    });                                   
                                }

                                

                                await Client.SendTextMessageAsync(
                                    chatId: chatid,
                                    text: "Выберите заказ",
                                    replyMarkup: keyboard,
                                    cancellationToken: cancellationToken);
                            }
                        }
                        else
                        {
                            TelegramWorker.Logger.LogError(
                                $"MainCommand \tНе существует объект {nameof(orders)}", cancellationToken);
                        }
                    }
                }
                else
                {
                    var keyboardUserNotExist = new InlineKeyboardMarkup(
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("Регистрация", "signUp"),
                        });

                    await Client.SendTextMessageAsync(
                        chatId: chatid,
                        text: "Для продолжения, зарегистрируйтесь",
                        replyMarkup: keyboardUserNotExist,
                        cancellationToken: cancellationToken);
                }
            }
        }
    }

    public async Task<bool> CheckCustomer(long chatid, CancellationToken cancellationToken)
    {
        if (!cancellationToken.IsCancellationRequested)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                var responce = await httpClient.PostAsync("http://localhost:5550/api/Customer/GetCustomers", new StringContent(""), cancellationToken);
                var responseJsonContent = await responce.Content.ReadAsStringAsync(cancellationToken);
                GetCustomerModel[] customers = JsonConvert.DeserializeObject<GetCustomerModel[]>(responseJsonContent);

                if (customers != null)
                {
                    foreach (var customer in customers)
                    {
                        try
                        {
                            if (long.Parse(customer.TelegramId) == chatid)
                            {
                                return true;
                            }
                        }
                        catch (FormatException ex)
                        {
                            TelegramWorker.Logger
                                 .LogWarning("Main command\n" +
                                "\tНевозможно распарсить идентификатор, скорее всего он не равен типу long", cancellationToken);
                        }

                    }
                }

                return false;
            }
        }

        return false;

    }
}
