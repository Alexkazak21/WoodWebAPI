using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace WoodWebAPI.Worker.Commands;

public class CancelCommand : ICommand
{
    public TelegramBotClient Client => TelegramWorker.API;
    public string Name => "/cancel";
    public async Task Execute(Update update, CancellationToken cancellationToken)
    {
        if (!cancellationToken.IsCancellationRequested)
        {
            var chatId = 0l;
            var messageId = 0;
            if (update.Type == UpdateType.CallbackQuery)
            {
                messageId = update.CallbackQuery.Message.MessageId;
                chatId = update.CallbackQuery.From.Id;
                await Client.EditMessageTextAsync(
                        chatId: chatId,
                        text: "Операция отменена. Вы вернётесь в начало",
                        messageId: messageId,
                        replyMarkup: new InlineKeyboardMarkup(
                                    new[]
                                    {
                                        InlineKeyboardButton.WithCallbackData("В начало","/start"),
                                        InlineKeyboardButton.WithCallbackData("Главное меню","/main"),
                                    }),
                        cancellationToken: cancellationToken
                        );
            }
            else if (update.Type == UpdateType.Message)
            {
                messageId = update.Message.MessageId;
                chatId = update.Message.From.Id;

                await Client.SendTextMessageAsync(
                        chatId: chatId,
                        text: "Операция отменена. Вы вернётесь в начало",
                        replyMarkup: new InlineKeyboardMarkup(
                                    new[]
                                    {
                                        InlineKeyboardButton.WithCallbackData("В начало","/start"),
                                        InlineKeyboardButton.WithCallbackData("Главное меню","/main"),
                                    }),
                        cancellationToken: cancellationToken
                        );
            }


        }
        else
        {
            return;
        }
    }
}
