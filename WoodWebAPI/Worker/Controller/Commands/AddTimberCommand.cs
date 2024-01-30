using Newtonsoft.Json;
using System.Globalization;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using WoodWebAPI.Data.Models;
using WoodWebAPI.Data.Models.Timber;

namespace WoodWebAPI.Worker.Controller.Commands;

public class AddTimberCommand : ICommand
{
    public TelegramBotClient Client => TelegramWorker.API;

    public string Name => "/add_timber";

    public async Task Execute(Update update, CancellationToken cancellationToken)
    {
        if (!cancellationToken.IsCancellationRequested)
        {
            if (update != null)
            {
                var userExist = false;

                long chatid = -1;

                int orderId = -1;

                int messageId = -1;

                int diameter = -1;

                double timberLength = -1.0;

                var comandParts = update?.CallbackQuery?.Data?.Split(':');

                if (update.Type == UpdateType.Message)
                {
                    chatid = update.Message.Chat.Id;
                    messageId = update.Message.MessageId;
                    userExist = await new CommonChecks().CheckCustomer(chatid, cancellationToken);
                }
                else if (update.Type == UpdateType.CallbackQuery)
                {
                    chatid = update.CallbackQuery.From.Id;
                    messageId = update.CallbackQuery.Message.MessageId;
                    userExist = await new CommonChecks().CheckCustomer(chatid, cancellationToken);
                }


                if (comandParts.Length == 2)
                {
                    try
                    {
                        int.TryParse(comandParts[1], out orderId);
                    }
                    catch (Exception ex)
                    {
                        TelegramWorker.Logger.LogError("Can`t get order id while executing delete command");
                    }

                    await Client.EditMessageTextAsync(
                                chatId: chatid,
                                text: "Введите длину и диаметр бревна по верхушке(конец с меньшим диаметром) в формате" +
                                "\n        диаметр:длинна" +
                                "\nДиаметр в сантиметрах, а длинна в метрах с точностью до 1см" +
                                "\n\tНапример  56:1.58" +
                                "\nОтправьте как ОТВЕТ на это сообщение" +
                                $"\nЗаказ - {orderId}",
                                messageId: messageId,
                                replyMarkup: null,
                                cancellationToken: cancellationToken);
                }

                if (comandParts.Length == 4)
                {
                    try
                    {
                        int.TryParse(comandParts[1], out orderId);
                        int.TryParse(comandParts[2], out diameter);
                        double.TryParse(comandParts[3], CultureInfo.InvariantCulture, out timberLength);
                    }
                    catch (Exception ex)
                    {
                        TelegramWorker.Logger.LogError("Wrong data when parsing information aboun adding timber to order");
                    }

                    if (userExist && chatid != -1 && orderId != -1 && diameter != -1 && timberLength != -1)
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
                                        AddTimberDTO addTimber = new AddTimberDTO()
                                        {
                                            OrderId = orderId,
                                            Length = timberLength,
                                            Diameter = diameter,
                                        };

                                        var content = JsonContent.Create(addTimber);

                                        var request = await httpClient.PostAsync($"{TelegramWorker.BaseUrl}/api/Timber/AddTimberToOrder", content);

                                        var responce = await request.Content.ReadAsStringAsync();
                                        var result = JsonConvert.DeserializeObject<ExecResultModel>(responce);
                                        if (result != null && result.Success)
                                        {
                                            await Client.SendTextMessageAsync(
                                                chatId: chatid,
                                                text: $"{result.Message}",
                                                replyMarkup: new InlineKeyboardMarkup(
                                                   InlineKeyboardButton.WithCallbackData("Вернуться к заказу", $"/show_order:{orderId}"))
                                                );
                                        }
                                        else if (result != null && !result.Success)
                                        {
                                            await Client.SendTextMessageAsync(
                                                chatId: chatid,
                                                text: $"{result.Message}",
                                                replyMarkup: new InlineKeyboardMarkup(
                                                   InlineKeyboardButton.WithCallbackData("Вернуться к заказу", $"/show_order:{orderId}"))
                                                );
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
