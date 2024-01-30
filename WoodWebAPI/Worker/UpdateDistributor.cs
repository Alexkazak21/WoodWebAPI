using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using WoodWebAPI.Data.Entities;

namespace WoodWebAPI.Worker
{
    public class UpdateDistributor<T> where T : IUpdateHandler, new()
    {
        private Dictionary<long, T> listeners;

        public UpdateDistributor()
        {
            listeners = new Dictionary<long, T>();
        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if( update.Message != null && update.CallbackQuery == null)
            {
                long chatId = update.Message.Chat.Id;

                CheckInnerMessageText(update);

                await SendUpdate(chatId, botClient, update, cancellationToken);
            }
            else if(update.CallbackQuery != null)
            {
                long chatId = long.Parse(update.CallbackQuery.Id);
                await SendUpdate(chatId, botClient, update, cancellationToken);
            }         
            
        }

        private async Task SendUpdate(long chatId, ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            T? listener = listeners.GetValueOrDefault(chatId);
            if (listener is null)
            {
                listener = new T();
                listeners.Add(chatId, listener);
                await listener.HandleUpdateAsync(botClient, update, cancellationToken);
                return;
            }

            await listener.HandleUpdateAsync(botClient, update, cancellationToken);
        }

        private void CheckInnerMessageText(Update update)
        {
            if (update.Message.ReplyToMessage != null && update.Message.ReplyToMessage.Text.StartsWith("Введите длину и диаметр бревна по верхушке"))
            {
                var textParams = update.Message.Text.Split(':');
                var orderId = int.Parse(update.Message.ReplyToMessage.Text.Split('-', StringSplitOptions.TrimEntries)[^1]);
                var diametr = textParams[0];
                var length = textParams[1];
                update.CallbackQuery = new CallbackQuery()
                {
                    Data = $"/add_timber:{orderId}:{diametr}:{length}",
                };
            }
            else if (update.Message.ReplyToMessage != null && update.Message.ReplyToMessage.Text.StartsWith("Введите пароль ОТВЕТОМ на это сообщение"))
            {
                var textParams = update.Message.Text;
                update.CallbackQuery = new CallbackQuery()
                {
                    Data = $"/reg_admin:{textParams}",
                };
            }
        }
    }
}   
