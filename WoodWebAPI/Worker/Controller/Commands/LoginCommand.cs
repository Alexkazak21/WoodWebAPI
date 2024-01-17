﻿using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types.Enums;

namespace WoodWebAPI.Worker.Controller.Commands;

public class LoginCommand : ICommand
{
    public TelegramBotClient Client => TelegramWorker.API;

    public string Name => "/login";

    public async Task Execute(Update update, CancellationToken cancellationToken)
    {
        WebAppInfo webAppInfo = new WebAppInfo();

        webAppInfo.Url = "https://woodcutters.mydurable.com/";

        var inlineMarkup = new InlineKeyboardMarkup(new[]
        {
           InlineKeyboardButton.WithWebApp(
                                text: "Продолжить в приложении",
                                webAppInfo),

           InlineKeyboardButton.WithCallbackData(
                                text: "Продолжить в боте",
                                callbackData: "/main"),
        });


        if (update.Type == UpdateType.CallbackQuery)
        {
            await Client.SendTextMessageAsync(
            chatId: update.CallbackQuery.From.Id,
            text: "Выберите вариант",
            replyMarkup: inlineMarkup);
        }
        else if (update.Type == UpdateType.Message)
        {
            await Client.SendTextMessageAsync(
            chatId: update.Message.Chat.Id,
            text: "Выберите вариант",
            replyMarkup: inlineMarkup); ;
        }
        
    }
}