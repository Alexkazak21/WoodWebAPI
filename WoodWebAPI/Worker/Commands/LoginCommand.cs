using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using WoodWebAPI.Data;
using WoodWebAPI.Data.Entities;

namespace WoodWebAPI.Worker.Commands;

public class LoginCommand(IWorkerCreds workerCreds, WoodDBContext wood) : ICommand
{
    private readonly IWorkerCreds _workerCreds = workerCreds;
    private readonly WoodDBContext _dbContext = wood;
    public TelegramBotClient Client => TelegramWorker.API;
    public string Name => "/login";

    public async Task Execute(Update update, CancellationToken cancellationToken)
    {
        try
        {
            WebAppInfo webAppInfo = new()
            {
                Url = "https://woodcutters.mydurable.com/"
            };

            var inlineMarkup = new InlineKeyboardMarkup(new[]
            {
                InlineKeyboardButton.WithWebApp(
                                text: "О нас",
                                webAppInfo),

                InlineKeyboardButton.WithCallbackData(
                                text: "Продолжить",
                                callbackData: "/main"),
            });


            var adminsList = await CommonChecks.GetAdmin(_workerCreds);

            if (adminsList != null)
            {
                for (int i = 0; i < adminsList.Length; i++)
                {
                    var admin = new IsAdmin
                    {
                        AdminRole = adminsList[i].AdminRole,
                        CreatedAt = adminsList[i].CreatedAt,
                        TelegramId = adminsList[i].TelegramId,
                        TelegramUsername = adminsList[i].TelegramUsername,
                        Id = adminsList[i].Id,
                    };

                    if (!_dbContext.IsAdmin.Contains(admin))
                    {
                        await _dbContext.IsAdmin.AddAsync(new IsAdmin
                        {
                            AdminRole = adminsList[i].AdminRole,
                            CreatedAt = adminsList[i].CreatedAt,
                            TelegramId = adminsList[i].TelegramId,
                            TelegramUsername = adminsList[i].TelegramUsername,
                            Id = adminsList[i].Id,
                        });
                        await _dbContext.SaveChangesAsync();
                    }
                }
            }


            if (update.Type == UpdateType.CallbackQuery)
            {
                await Client.EditMessageTextAsync(
                chatId: update.CallbackQuery.From.Id,
                messageId: update.CallbackQuery.Message.MessageId,
                text: "Добро пожаловать в систему\nВыберите вариант",
                replyMarkup: inlineMarkup);
            }
            else if (update.Type == UpdateType.Message)
            {
                await Client.SendTextMessageAsync(
                chatId: update.Message.Chat.Id,
                text: "Добро пожаловать в систему\nВыберите вариант",
                replyMarkup: inlineMarkup); ;
            }
        }
        catch (Exception)
        {
            if (update.Type == UpdateType.CallbackQuery)
            {
                await Client.EditMessageTextAsync(
                chatId: update.CallbackQuery.From.Id,
                messageId: update.CallbackQuery.Message.MessageId,
                text: "ОШИБКА попробуйте снова",
                replyMarkup: new InlineKeyboardMarkup(
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("В начало","/start")
                    }));
            }
            else if (update.Type == UpdateType.Message)
            {
                await Client.SendTextMessageAsync(
                chatId: update.Message.Chat.Id,
                text: "ОШИБКА попробуйте снова",
                replyMarkup: new InlineKeyboardMarkup(
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("В начало","/start")
                    }));
            }
        }
    }
}