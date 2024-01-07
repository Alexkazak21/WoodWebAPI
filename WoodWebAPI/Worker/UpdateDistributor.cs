using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using WoodWebAPI.Worker.Controller.Commands;

namespace WoodWebAPI.Worker
{
    public class UpdateDistributor<T> where T : IUpdateListener, new()
    {
        private Dictionary<long, T> listeners;

        public UpdateDistributor()
        {
            listeners = new Dictionary<long, T>();
        }

        public async Task GetUpdateAsync(Update update, CancellationToken cancellationToken)
        {
            long chatId = update.Message.Chat.Id;
            T? listener = listeners.GetValueOrDefault(chatId);
            if (listener is null)
            {
                listener = new T();
                listeners.Add(chatId, listener);
                await listener.GetUpdateAsync(update,cancellationToken);
                return;
            }
            await listener.GetUpdateAsync(update, cancellationToken);
        }
    }
}
