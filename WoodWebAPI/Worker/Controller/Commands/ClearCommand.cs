using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace WoodWebAPI.Worker.Controller.Commands
{
    public class ClearCommand : ICommand
    {
        public TelegramBotClient Client => TelegramWorker.API;

        public string Name => "/clear";

        public async Task Execute(Update update, CancellationToken cancellationToken)
        {
            if (!cancellationToken.IsCancellationRequested)
            {
                var chatId = 0l;

                if (update.Type == UpdateType.Message)
                {
                    chatId = update.Message.Chat.Id;
                    await Client.SendTextMessageAsync(
                    chatId: chatId,
                    text: "Это действие удалит все доступные системой сообщения" +
                    "\nПосле подтверждения дождитесь завершения операции и начните с начала",
                    replyMarkup: new InlineKeyboardMarkup(
                        inlineKeyboardButton: InlineKeyboardButton.WithCallbackData("Подтвердить", "/clear:true")),
                    cancellationToken: cancellationToken
                    );
                }
                else if (update.Type == UpdateType.CallbackQuery && update.CallbackQuery.Data == "/clear:true")
                {
                    while (true)
                    {
                        chatId = update.CallbackQuery.From.Id;
                        try
                        {
                            var currentMessage = update.CallbackQuery.Message.MessageId;
                            for (int i = 0; i < 1001; i++)
                            {
                                await Client.DeleteMessageAsync(
                                    chatId: chatId,
                                    messageId: currentMessage - i);
                            }
                        }
                        catch (Exception ex)
                        {
                            TelegramWorker.Logger.LogInformation(ex.Message, ex);
                        }

                        break;
                    }

                    await Client.SendTextMessageAsync(
                    chatId: chatId,
                    text: "Удаление завершено",
                    replyMarkup: new InlineKeyboardMarkup(
                        inlineKeyboardButton: InlineKeyboardButton.WithCallbackData("В начало", "/start")),
                    cancellationToken: cancellationToken);

                }
            }
        }
    }
}
