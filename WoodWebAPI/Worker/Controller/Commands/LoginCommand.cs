using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using WoodWebAPI.Data.Entities;

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
                                text: "О нас",
                                webAppInfo),

                InlineKeyboardButton.WithCallbackData(
                                text: "Продолжить",
                                callbackData: "/main"),
           });


        var adminsList = await new CommonChecks().GetAdmin();

        if (adminsList != null)
        {
            for (int i = 0; i < adminsList.Length; i++)
            {
                if (!TelegramWorker.AdminList.Contains(new IsAdmin
                {
                    AdminRole = adminsList[i].AdminRole,
                    CreatedAt = adminsList[i].CreatedAt,
                    TelegramId = adminsList[i].TelegramId,
                    TelegramUsername = adminsList[i].TelegramUsername,
                    Id = adminsList[i].Id,
                }))
                {
                    TelegramWorker.AdminList.Add(new IsAdmin
                    {
                        AdminRole = adminsList[i].AdminRole,
                        CreatedAt = adminsList[i].CreatedAt,
                        TelegramId = adminsList[i].TelegramId,
                        TelegramUsername = adminsList[i].TelegramUsername,
                        Id = adminsList[i].Id,
                    });
                }                
            }
        }

        if (update.Type == UpdateType.CallbackQuery)
        {
            await Client.EditMessageTextAsync(
            chatId: update.CallbackQuery.From.Id,
            messageId: update.CallbackQuery.Message.MessageId,
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
