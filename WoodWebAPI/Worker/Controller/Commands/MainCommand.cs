using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

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

                var messageId = 0;

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

                if (userExist && chatid != -1)
                {
                    var orders = await new CommonChecks().CheckOrdersOfCustomer(chatid, cancellationToken);

                    if (orders != null)
                    {
                        if (orders.Length == 0)
                        { 
                            await Client.EditMessageTextAsync(
                                chatId: chatid,
                                text: "У вас пока нет заказов. \nХотите создать заказ?",
                                messageId: messageId,
                                replyMarkup: new InlineKeyboardMarkup(
                                                new[]
                                                {
                                                    InlineKeyboardButton.WithCallbackData("Создать заказ","/new_order"),
                                                }),
                                cancellationToken: cancellationToken);
                        }
                        else if (orders.Length >= 5)
                        {
                            await Client.EditMessageTextAsync(
                                chatId: chatid,
                                text: "Максимальное колличество одновременных заказов 4",
                                messageId: messageId,
                                replyMarkup: new InlineKeyboardMarkup(
                                                new[]
                                                {
                                                    InlineKeyboardButton.WithCallbackData("К заказам","/main"),
                                                }),
                                cancellationToken: cancellationToken);
                        }
                        else if (orders != null && orders.Length < 5)
                        {
                            var keybordButtons = new List<InlineKeyboardButton>();
                            for (int i = 0; i < orders.Length; i++)
                            {
                                keybordButtons.Add(
                                    InlineKeyboardButton.WithCallbackData($"{orders[i].Id}", $"/show_order:{orders[i].Id}"));

                            }

                            InlineKeyboardMarkup? keyboard = null;
                            if (orders.Length < 4)
                            {
                                keyboard = new InlineKeyboardMarkup(new[]
                                {
                                        keybordButtons.ToArray<InlineKeyboardButton>(),
                                        [
                                            InlineKeyboardButton.WithCallbackData("Добавить заказ", "/new_order"),
                                            ],
                                        [
                                            InlineKeyboardButton.WithCallbackData("Удалить заказ", "/delete_order"),
                                            ]
                                    });
                            }
                            else
                            {
                                keyboard = new InlineKeyboardMarkup(new[]
                                {
                                        keybordButtons.ToArray<InlineKeyboardButton>(),
                                        [
                                            InlineKeyboardButton.WithCallbackData("Удалить заказ", "/delete_order"),
                                        ]
                                    });
                            }



                            if (update.Type == UpdateType.CallbackQuery)
                            {
                                await Client.EditMessageTextAsync(
                                chatId: chatid,
                                text: "Выберите заказ",
                                messageId: update.CallbackQuery.Message.MessageId,
                                replyMarkup: keyboard,
                                cancellationToken: cancellationToken);
                            }
                            else
                            {
                                await Client.SendTextMessageAsync(
                                                                chatId: chatid,
                                                                text: "Выберите заказ",
                                                                replyMarkup: keyboard,
                                                                cancellationToken: cancellationToken);
                            }                            
                        }
                    }
                    else
                    {
                        TelegramWorker.Logger.LogError(
                            $"MainCommand \tНе существует объект {nameof(orders)}", cancellationToken);
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
}
