using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using WoodWebAPI.Data.Entities;

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

                bool continueAsUser = false;
                bool isAdmin = false;

                string[]? commandParts = null;
                if (update.Type == UpdateType.Message)
                {
                    chatid = update.Message.Chat.Id;

                    messageId = update.Message.MessageId;

                    if (TelegramWorker.AdminList.Find(x => x.TelegramId == chatid.ToString()) != null) { isAdmin = true; }

                    userExist = await new CommonChecks().CheckCustomer(chatid, cancellationToken);
                }
                else if (update.Type == UpdateType.CallbackQuery)
                {
                    chatid = update.CallbackQuery.From.Id;

                    messageId = update.CallbackQuery.Message.MessageId;

                    if (TelegramWorker.AdminList.Find(x => x.TelegramId == chatid.ToString()) != null) { isAdmin = true; }

                    userExist = await new CommonChecks().CheckCustomer(chatid, cancellationToken);

                    commandParts = update.CallbackQuery.Data.Split(":");

                   if( commandParts.Length > 1  && commandParts[1] == "true")
                   {
                        continueAsUser = true;
                   }
                }

                if ((userExist && chatid != -1 && !isAdmin) || continueAsUser)
                {
                    var orders = await new CommonChecks().CheckOrdersOfCustomer(chatid, cancellationToken);

                    if (orders != null)
                    {
                        if (orders.Length == 0)
                        {
                            try
                            {
                                await Client.EditMessageTextAsync(
                                chatId: chatid,
                                text: "У вас пока нет заказов. \nХотите создать заказ?",
                                messageId: messageId,
                                replyMarkup: new InlineKeyboardMarkup(
                                                new[]
                                                {
                                                    new[]
                                                    {
                                                        InlineKeyboardButton.WithCallbackData("Создать заказ","/new_order"),
                                                    },
                                                    new[]
                                                    {
                                                        InlineKeyboardButton.WithCallbackData("Главное меню","/main"),
                                                    }
                                                }),
                                cancellationToken: cancellationToken);
                            }
                            catch (ApiRequestException ex) 
                            {
                                await Client.EditMessageTextAsync(
                               chatId: chatid,
                               text: "Вы и так находитесь в Главном меню" +
                               "\nУ вас пока нет заказов. " +
                               "\nХотите создать заказ?",
                               messageId: messageId,
                               replyMarkup: new InlineKeyboardMarkup(
                                               new[]
                                               {
                                                    new[]
                                                    {
                                                        InlineKeyboardButton.WithCallbackData("Создать заказ","/new_order"),
                                                    }
                                               }),
                               cancellationToken: cancellationToken);
                            }
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
                else if (userExist && chatid != -1 && isAdmin)
                {
                    await Client.DeleteMyCommandsAsync(BotCommandScope.Chat(chatid));

                    var commands = new List<BotCommand>()
                    {
                        new BotCommand()
                        {
                            Command = "start",
                            Description = "В начало"
                        },
                        new BotCommand() 
                        {
                            Command = "order_manage",
                            Description = "Управление заказами"
                        },
                        new BotCommand()
                        {
                            Command = "login",
                            Description = "Авторизация"
                        },
                        new BotCommand()
                        {
                            Command = "main",
                            Description = "В главное меню"
                        },
                        new BotCommand()
                        {
                            Command = "cancel",
                            Description = "Отменить действие"
                        },
                        new BotCommand()
                        {
                            Command = "clear",
                            Description = "Удалить сообщения"
                        }
                    };

                    await Client.SetMyCommandsAsync(commands, BotCommandScope.Chat(chatid));

                    InlineKeyboardMarkup keyboard = new InlineKeyboardMarkup(                            
                            new[]
                            {
                                new[]
                                {
                                    InlineKeyboardButton.WithCallbackData("Управление заказами", "/order_manage"),
                                },
                                new[]
                                {
                                     InlineKeyboardButton.WithCallbackData("Продолжить как пользователь", "/main:true"),
                                }                                
                            });
                    if (update.Type == UpdateType.CallbackQuery)
                    {
                        await Client.EditMessageTextAsync(
                                            chatId: chatid,
                                            text: "Выберите операцию",
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
