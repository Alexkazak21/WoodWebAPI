using Microsoft.AspNetCore.Http.HttpResults;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using WoodWebAPI.Data.Models.OrderPosition;

namespace WoodWebAPI.Worker.Controller.Commands
{
    public class AlterOrderPositionCommand : ICommand
    {
        public TelegramBotClient Client => TelegramWorker.API;

        public string Name => "/alter_timber";

        public async Task Execute(Update update, CancellationToken cancellationToken)
        {
            if (!cancellationToken.IsCancellationRequested)
            {
                if (update == null)
                {
                    return;
                }

                try
                {
                    var chatId = update.CallbackQuery.From.Id;
                    var comandParts = update.CallbackQuery.Data.Split(':');
                    var orderId = -1;
                    int.TryParse(comandParts[1], out orderId);

                    var timberList = await GetOrderPositionsList(orderId, chatId);

                    if (comandParts.Length == 2 && orderId > 0)
                    {
                        if (timberList.OrderPositions.Count > 0)
                        {
                            InlineKeyboardMarkup replyMarkup = null;
                            if (timberList.OrderPositions.Count > 1)
                            {
                                replyMarkup = new InlineKeyboardMarkup(
                                new[]
                                {
                                    new[]
                                    {
                                        InlineKeyboardButton.WithCallbackData("Изменить",$"/alter_timber:{orderId}:{timberList.OrderPositions[0].OrderPositionId}:true"),
                                        InlineKeyboardButton.WithCallbackData(">",$"/alter_timber:{orderId}:{timberList.OrderPositions[1].OrderPositionId}")
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
                                        InlineKeyboardButton.WithCallbackData("Изменить",$"/alter_timber:{orderId}:{timberList.OrderPositions[0].OrderPositionId}:true")
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
                                                              $"\nДиаметр: {timberList.OrderPositions[0].DiameterInCantimeter} см" +
                                                              $"\nДлина: {timberList.OrderPositions[0].LengthInMeter} м" +
                                                              $"\nОбъёмом: {timberList.OrderPositions[0].VolumeInMeter3} м3",
                                                        messageId: update.CallbackQuery.Message.MessageId,
                                                        replyMarkup: replyMarkup,
                                                        cancellationToken: cancellationToken);
                        }
                    }
                    else if (comandParts.Length == 3 && orderId > 0)
                    {
                        var timberId = int.Parse(comandParts[2]);
                        int currentTimber = -1;

                        for (int i = 0; i < timberList.OrderPositions.Count; i++)
                        {
                            if (timberId == timberList.OrderPositions[i].OrderPositionId)
                            {
                                currentTimber = i;
                                break;
                            }
                        }

                        if (timberId == timberList.OrderPositions[0].OrderPositionId)
                        {
                            InlineKeyboardMarkup replyMarkup;
                            if (timberList.OrderPositions.Count > 1)
                            {
                                replyMarkup = new InlineKeyboardMarkup(
                                new[]
                                {
                                    new[]
                                    {
                                    InlineKeyboardButton.WithCallbackData("Изменить",$"/alter_timber:{orderId}:{timberList.OrderPositions[currentTimber].OrderPositionId}:true"),
                                    InlineKeyboardButton.WithCallbackData(">",$"/alter_timber:{orderId}:{timberList.OrderPositions[currentTimber + 1].OrderPositionId}")
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
                                        InlineKeyboardButton.WithCallbackData("Изменить",$"/alter_timber:{orderId}:{timberList.OrderPositions[currentTimber].OrderPositionId}:true"),
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
                                                              $"\nДиаметр: {timberList.OrderPositions[currentTimber].DiameterInCantimeter} см" +
                                                              $"\nДлина: {timberList.OrderPositions[currentTimber].LengthInMeter} м" +
                                                              $"\nОбъёмом: {timberList.OrderPositions[currentTimber].VolumeInMeter3} м3",
                                                        messageId: update.CallbackQuery.Message.MessageId,
                                                        replyMarkup: replyMarkup,
                                                        cancellationToken: cancellationToken);
                        }
                        else if (timberId == timberList.OrderPositions[^1].OrderPositionId)
                        {
                            InlineKeyboardMarkup replyMarkup = new InlineKeyboardMarkup(
                                new[]
                                {
                                    new[]
                                    {
                                        InlineKeyboardButton.WithCallbackData("<",$"/alter_timber:{orderId}:{timberList.OrderPositions[currentTimber - 1].OrderPositionId}"),
                                        InlineKeyboardButton.WithCallbackData("Изменить",$"/alter_timber:{orderId}:{timberList.OrderPositions[currentTimber].OrderPositionId}:true"),
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
                                                              $"\nДиаметр: {timberList.OrderPositions[currentTimber].DiameterInCantimeter} см" +
                                                              $"\nДлина: {timberList.OrderPositions[currentTimber].LengthInMeter} м" +
                                                              $"\nОбъёмом: {timberList.OrderPositions[currentTimber].VolumeInMeter3} м3",
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
                                        InlineKeyboardButton.WithCallbackData("<",$"/alter_timber:{orderId}:{timberList.OrderPositions[currentTimber - 1].OrderPositionId}"),
                                        InlineKeyboardButton.WithCallbackData("Изменить",$"/alter_timber:{orderId}:{timberList.OrderPositions[currentTimber].OrderPositionId}:true"),
                                        InlineKeyboardButton.WithCallbackData(">",$"/alter_timber:{orderId}:{timberList.OrderPositions[currentTimber + 1].OrderPositionId}"),
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
                                                              $"\nДиаметр: {timberList.OrderPositions[currentTimber].DiameterInCantimeter} см" +
                                                              $"\nДлина: {timberList.OrderPositions[currentTimber].LengthInMeter} м" +
                                                              $"\nОбъёмом: {timberList.OrderPositions[currentTimber].VolumeInMeter3} м3",
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
                catch(ApiRequestException badException)
                {
                    TelegramWorker.Logger.LogWarning($"Source: {badException.Source}\n" +
                        $"\tError code:{badException.ErrorCode}\n" +
                        $"\tMessage:\n\t{badException.Message}");
                }
                
            }
        }

        private async Task<OrderPositionsModel?> GetOrderPositionsList(int orderId, long chatId)
        {
            using HttpClient httpClient = new HttpClient();

            var content = JsonContent.Create(
                new GetOrderPositionsByOrderIdDTO()
                {
                    OrderId = orderId,
                    TelegramId = chatId,
                }
             );

            var request = await httpClient.PostAsync($"{TelegramWorker.BaseUrl}/api/OrderPosition/GetOrderPositionsOfOrder", content);
            var responce = await request.Content.ReadAsStringAsync();

            var result = JsonConvert.DeserializeObject<OrderPositionsModel>(responce);

            return result;
        }
    }
}
