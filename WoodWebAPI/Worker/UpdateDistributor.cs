using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;

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

                //if (update.Message.ReplyToMessage != null && update.Message.ReplyToMessage.Text == "Введите пароль! ( Ответом на это сообщение)")
                //{
                //    CallbackQuery callbackQuery = new CallbackQuery()
                //    {
                //        Data = "signUp",
                //        Id = new Random().NextInt64().ToString(),
                //        From = update.Message.From,
                //        Message = new Message()
                //        {
                //            Text = "Пользовательский пароль",
                //        },
                //    };

                //    update.CallbackQuery = callbackQuery;
                //    await SendUpdate(chatId, botClient, update, cancellationToken);
                //}
                //else
                //{
                    await SendUpdate(chatId, botClient, update, cancellationToken);
                //}
                
               
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
    }
}
