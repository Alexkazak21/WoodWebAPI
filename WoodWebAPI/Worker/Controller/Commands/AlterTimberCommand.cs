using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using WoodWebAPI.Data.Models.Timber;

namespace WoodWebAPI.Worker.Controller.Commands
{
    public class AlterTimberCommand : ICommand
    {
        public TelegramBotClient Client => TelegramWorker.API;

        public string Name => "/alter_timber";

        public async Task Execute(Update update, CancellationToken cancellationToken)
        {
            if (!cancellationToken.IsCancellationRequested)
            {
                if (update != null)
                {
                    var chatId = update.CallbackQuery.From.Id;
                    var comandParts = update.CallbackQuery.Data.Split(':');
                    var orderId = -1;
                    int.TryParse(comandParts[1], out orderId);

                    var timberList = await GetTimbersList(orderId, chatId);

                    if (comandParts.Length == 2 && orderId > 0)
                    {
                        if (timberList.Timbers.Count > 0)
                        {
                            InlineKeyboardMarkup replyMarkup = null;
                            if (timberList.Timbers.Count > 1)
                            {
                                replyMarkup = new InlineKeyboardMarkup(
                                new[]
                                {
                                    new[]
                                    {
                                        InlineKeyboardButton.WithCallbackData("Изменить",$"/alter_timber:{orderId}:{timberList.Timbers[0].Id}:true"),
                                    InlineKeyboardButton.WithCallbackData(">",$"/alter_timber:{orderId}:{timberList.Timbers[1].Id}")
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
                                        InlineKeyboardButton.WithCallbackData("Изменить",$"/alter_timber:{orderId}:{timberList.Timbers[0].Id}:true")
                                    },
                                    new[]
                                    {
                                        InlineKeyboardButton.WithCallbackData("К заказам","/main")
                                    }
                                });

                            }


                            await Client.EditMessageTextAsync(
                                                        chatId: chatId,
                                                        text: $"Ваш заказ номер {orderId}" +
                                                              $"\nБревно № {1}" +
                                                              $"\nДиаметр: {timberList.Timbers[0].Diameter} см" +
                                                              $"\nДлина: {timberList.Timbers[0].Length} м" +
                                                              $"\nОбъёмом: {timberList.Timbers[0].Volume} m3",
                                                        messageId: update.CallbackQuery.Message.MessageId,
                                                        replyMarkup: replyMarkup,
                                                        cancellationToken: cancellationToken);
                        }
                    }
                    else if (comandParts.Length == 3 && orderId > 0)
                    {
                        var timberId = int.Parse(comandParts[2]);
                        int currentTimber = -1;

                        for (int i = 0; i < timberList.Timbers.Count; i++)
                        {
                            if (timberId == timberList.Timbers[i].Id)
                            {
                                currentTimber = i;
                                break;
                            }
                        }

                        if (timberId == timberList.Timbers[0].Id)
                        {
                            InlineKeyboardMarkup replyMarkup = null;
                            if (timberList.Timbers.Count > 1)
                            {
                                replyMarkup = new InlineKeyboardMarkup(
                                new[]
                                {
                                    InlineKeyboardButton.WithCallbackData("Изменить",$"/alter_timber:{orderId}:{timberList.Timbers[currentTimber].Id}:true"),
                                    InlineKeyboardButton.WithCallbackData(">",$"/alter_timber:{orderId}:{timberList.Timbers[currentTimber + 1].Id}")
                                });
                            }
                            else
                            {
                                replyMarkup = new InlineKeyboardMarkup(
                                new[]
                                {
                                    new[]
                                    {
                                        InlineKeyboardButton.WithCallbackData("Изменить",$"/alter_timber:{orderId}:{timberList.Timbers[currentTimber].Id}:true"),
                                    },
                                    new[]
                                    {
                                        InlineKeyboardButton.WithCallbackData("К заказам","/main")
                                    }
                                });
                            }

                            await Client.EditMessageTextAsync(
                                                        chatId: chatId,
                                                        text: $"Ваш заказ номер {orderId}" +
                                                              $"\nБревно № {currentTimber + 1}" +
                                                              $"\nДиаметр: {timberList.Timbers[currentTimber].Diameter} см" +
                                                              $"\nДлина: {timberList.Timbers[currentTimber].Length} м" +
                                                              $"\nОбъёмом: {timberList.Timbers[currentTimber].Volume} m3",
                                                        messageId: update.CallbackQuery.Message.MessageId,
                                                        replyMarkup: replyMarkup,
                                                        cancellationToken: cancellationToken);
                        }
                        else if (timberId == timberList.Timbers[^1].Id)
                        {
                            InlineKeyboardMarkup replyMarkup = new InlineKeyboardMarkup(
                                new[]
                                {
                                    new[]
                                    {
                                        InlineKeyboardButton.WithCallbackData("<",$"/alter_timber:{orderId}:{timberList.Timbers[currentTimber - 1].Id}"),
                                        InlineKeyboardButton.WithCallbackData("Изменить",$"/alter_timber:{orderId}:{timberList.Timbers[currentTimber].Id}:true"),
                                    },
                                    new[]
                                    {
                                        InlineKeyboardButton.WithCallbackData("К заказам","/main")
                                    }
                                });


                            await Client.EditMessageTextAsync(
                                                        chatId: chatId,
                                                        text: $"Ваш заказ номер {orderId}" +
                                                              $"\nБревно № {currentTimber + 1}" +
                                                              $"\nДиаметр: {timberList.Timbers[currentTimber].Diameter} см" +
                                                              $"\nДлина: {timberList.Timbers[currentTimber].Length} м" +
                                                              $"\nОбъёмом: {timberList.Timbers[currentTimber].Volume} m3",
                                                        messageId: update.CallbackQuery.Message.MessageId,
                                                        replyMarkup: replyMarkup,
                                                        cancellationToken: cancellationToken);
                        }
                        else
                        {
                            InlineKeyboardMarkup replyMarkup = new InlineKeyboardMarkup(
                                new[]
                                {
                                    new[]
                                    {
                                        InlineKeyboardButton.WithCallbackData("<",$"/alter_timber:{orderId}:{timberList.Timbers[currentTimber - 1].Id}"),
                                        InlineKeyboardButton.WithCallbackData("Изменить",$"/alter_timber:{orderId}:{timberList.Timbers[currentTimber].Id}:true"),
                                        InlineKeyboardButton.WithCallbackData(">",$"/alter_timber:{orderId}:{timberList.Timbers[currentTimber + 1].Id}"),
                                    },
                                    new[]
                                    {
                                        InlineKeyboardButton.WithCallbackData("К заказам","/main")
                                    }
                                });


                            await Client.EditMessageTextAsync(
                                                        chatId: chatId,
                                                        text: $"Ваш заказ номер {orderId}" +
                                                              $"\nБревно № {currentTimber + 1}" +
                                                              $"\nДиаметр: {timberList.Timbers[currentTimber].Diameter} см" +
                                                              $"\nДлина: {timberList.Timbers[currentTimber].Length} м" +
                                                              $"\nОбъёмом: {timberList.Timbers[currentTimber].Volume} m3",
                                                        messageId: update.CallbackQuery.Message.MessageId,
                                                        replyMarkup: replyMarkup,
                                                        cancellationToken: cancellationToken);
                        }
                    }
                    else if (comandParts.Length == 4 && orderId > 0)
                    {
                        InlineKeyboardMarkup replyMarkup = new InlineKeyboardMarkup(
                                new[]
                                {
                                    new[]
                                    {
                                        InlineKeyboardButton.WithCallbackData("К заказам","/main")
                                    }
                                });


                        await Client.EditMessageTextAsync(
                                                    chatId: chatId,
                                                    text: "Пока ничего нет, вернитесь к заказам",
                                                    messageId: update.CallbackQuery.Message.MessageId,
                                                    replyMarkup: replyMarkup,
                                                    cancellationToken: cancellationToken);
                    }
                }
            }
        }

        private async Task<GetTimber> GetTimbersList(int orderId, long chatId)
        {
            using HttpClient httpClient = new HttpClient();

            var content = JsonContent.Create(
                new GetTimberDTO()
                {
                    OrderId = orderId,
                    customerTelegramId = chatId.ToString(),
                }
             );

            var request = await httpClient.PostAsync($"{TelegramWorker.BaseUrl}/api/Timber/GetTimbersOfOrder", content);
            var responce = await request.Content.ReadAsStringAsync();

            var result = JsonConvert.DeserializeObject<GetTimber>(responce);

            return result;
        }
    }
}
