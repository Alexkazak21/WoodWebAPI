using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using WoodWebAPI.Data.Models;
using WoodWebAPI.Data.Models.Order;

namespace WoodWebAPI.Worker.Controller.Commands;

public class DeleteOrderCommand : ICommand
{
    public TelegramBotClient Client => TelegramWorker.API;

    public string Name => "/delete_order";

    public async Task Execute(Update update, CancellationToken cancellationToken)
    {
        if (!cancellationToken.IsCancellationRequested)
        {
            if (update != null)
            {
                int orderId = -1;
                try
                {
                    int.TryParse(update?.CallbackQuery?.Data?.Substring(update.CallbackQuery.Data.IndexOf(':') + 1, update.CallbackQuery.Data.Length - 1 - update.CallbackQuery.Data.IndexOf(':')), out orderId);
                }
                catch (Exception ex)
                {
                    TelegramWorker.Logger.LogError("Can`t get order id while executing delete command");
                }

                var userExist = false;

                long chatid = -1;

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

                if (userExist && chatid != -1 && orderId > 0)
                {
                    using (HttpClient httpClient = new HttpClient())
                    {
                        var content = JsonContent.Create(
                            new DeleteOrderDTO()
                            {
                                CustomerTelegramId = chatid.ToString(),
                                OrderId = orderId,
                            });

                        var request = await httpClient.PostAsync($"{TelegramWorker.BaseUrl}/api/Order/DeleteOrder", content, cancellationToken);
                        
                        var response = await request.Content.ReadAsStringAsync(cancellationToken);

                        var result = JsonConvert.DeserializeObject<ExecResultModel>(response);

                        if(result != null && result.Success)
                        {
                            await Client.EditMessageTextAsync(
                            chatId: chatid,
                            text: $"{result.Message}",
                            messageId: update.CallbackQuery.Message.MessageId,
                            replyMarkup: new InlineKeyboardMarkup(
                                                    new[]
                                                    {
                                                    InlineKeyboardButton.WithCallbackData("К заказам","/main"),
                                                    })
                            );
                        }
                        else if(result != null && !result.Success)
                        {
                            await Client.EditMessageTextAsync(
                                chatId: chatid,
                                text: $"{response}",
                                messageId: update.CallbackQuery.Message.MessageId,
                                replyMarkup: new InlineKeyboardMarkup(
                                                        new[]
                                                        {
                                                    InlineKeyboardButton.WithCallbackData("К заказам","/main"),
                                                        })
                                );
                        }
                        else 
                        {
                            await Client.EditMessageTextAsync(
                            chatId: chatid,
                            text: $"Произохла непредвиденная ошибка, попытайтесь снова",
                            messageId: update.CallbackQuery.Message.MessageId,
                            replyMarkup: new InlineKeyboardMarkup(
                                                    new[]
                                                    {
                                                    InlineKeyboardButton.WithCallbackData("К заказам","/main"),
                                                    })
                            );
                        }
                    }
                }
                else if (orderId <= 0 )
                {
                    var orders = await new CommonChecks().CheckOrdersOfCustomer(chatid, cancellationToken);

                    if (orders != null && orders.Length > 0)
                    {
                        var keybordButtons = new List<InlineKeyboardButton>();
                        for (int i = 0; i < orders.Length; i++)
                        {
                            keybordButtons.Add(
                                InlineKeyboardButton.WithCallbackData($"{orders[i].Id}", $"/delete_order:{orders[i].Id}"));

                        }

                        InlineKeyboardMarkup? keyboard = null;

                        keyboard = new InlineKeyboardMarkup(new[]
                        {
                            keybordButtons.ToArray<InlineKeyboardButton>()                                
                        });

                        await Client.EditMessageTextAsync(
                                chatId: chatid,
                                text: "Выберите заказ который хотите удалить",
                                messageId: update.CallbackQuery.Message.MessageId,
                                replyMarkup: keyboard,
                                cancellationToken: cancellationToken);
                    }
                }
            }
        }
    }
}
