using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using WoodWebAPI.Data.Models;
using WoodWebAPI.Data.Models.Timber;

namespace WoodWebAPI.Worker.Controller.Commands
{
    public class ShowOrderCommand : ICommand
    {
        public TelegramBotClient Client => TelegramWorker.API;

        public string Name => "/show_order";

        public async Task Execute(Update update, CancellationToken cancellationToken)
        {
            if (!cancellationToken.IsCancellationRequested)
            {
                if (update != null)
                {
                    var userExist = false;

                    long chatid = -1;

                    int orderId = -1;
                    try
                    {
                        int.TryParse(update?.CallbackQuery?.Data?.Substring(update.CallbackQuery.Data.IndexOf(':') + 1, update.CallbackQuery.Data.Length - 1 - update.CallbackQuery.Data.IndexOf(':')), out orderId);
                    }
                    catch (Exception ex)
                    {
                        TelegramWorker.Logger.LogError("Can`t get order id while executing delete command");
                    }

                    if (update.Type == UpdateType.Message)
                    {
                        chatid = update.Message.Chat.Id;

                        userExist = await new CommonChecks().CheckCustomer(chatid, cancellationToken);
                    }
                    else if (update.Type == UpdateType.CallbackQuery)
                    {
                        chatid = update.CallbackQuery.From.Id;

                        userExist = await new CommonChecks().CheckCustomer(chatid, cancellationToken);
                    }

                    if (userExist && chatid != -1 && orderId != -1)
                    {
                        var orders = await new CommonChecks().CheckOrdersOfCustomer(chatid, cancellationToken);

                        if (orders != null)
                        {
                            foreach (var order in orders)
                            {
                                if (order.Id == orderId)
                                {
                                    var volume = 0.0;
                                    using (HttpClient httpClient = new HttpClient())
                                    {
                                        GetTimberDTO getTimbers = new GetTimberDTO()
                                        {
                                            customerTelegramId = chatid.ToString(),
                                            OrderId = orderId,
                                        };

                                        var content = JsonContent.Create(getTimbers);

                                        var request = await httpClient.PostAsync($"{TelegramWorker.BaseUrl}/api/Timber/GetTotalVolumeOfOrder", content);

                                        var responce = await request.Content.ReadAsStringAsync();
                                        var result = JsonConvert.DeserializeObject<ExecResultModel>(responce);
                                        if (result != null && result.Success)
                                        {
                                            volume = double.Parse(result.Message.Split(' ')[8]);
                                        }
                                    }

                                    var orderIsVerified = order.IsVerified ? "ДА" : "НЕТ";

                                    InlineKeyboardMarkup replyMarkup = null;
                                    if (volume == 0.0)
                                    {
                                        replyMarkup = new InlineKeyboardMarkup(
                                                        new[]
                                                            {
                                                                new[]
                                                                {             
                                                                    InlineKeyboardButton.WithCallbackData("Добавить бревно",$"/add_timber:{order.Id}")
                                                                },
                                                                new[]
                                                                {
                                                                    InlineKeyboardButton.WithCallbackData("К заказам","/main")
                                                                }
                                                        });
                                    }
                                    else
                                    {
                                        replyMarkup = new InlineKeyboardMarkup(
                                                        new[]
                                                        {
                                                            new[]
                                                            {
                                                                InlineKeyboardButton.WithCallbackData("Добавить бревно",$"/add_timber:{order.Id}")                                                                
                                                            },
                                                            new[]
                                                            {
                                                                InlineKeyboardButton.WithCallbackData("Изменить описание брёвен",$"/alter_timber:{orderId}")
                                                            },
                                                            new[]
                                                            {
                                                                InlineKeyboardButton.WithCallbackData("К заказам","/main")
                                                            }
                                                        });
                                    }

                                    await Client.EditMessageTextAsync(
                                                    chatId: chatid,
                                                    text: $"Ваш заказ номер {order.Id}" +
                                                          $"\nДата создания: {order.CreatedAt}" +
                                                          $"\nПодтверждён: {orderIsVerified}" +
                                                          $"\nОбъёмом: {volume} m3",
                                                    messageId: update.CallbackQuery.Message.MessageId,
                                                    replyMarkup: replyMarkup,
                                                    cancellationToken: cancellationToken);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
