using Telegram.Bot;
using Telegram.Bot.Types;

namespace WoodWebAPI.Worker.Controller.Commands
{
    public class AlterTimberCommand : ICommand
    {
        public TelegramBotClient Client => TelegramWorker.API;

        public string Name => "/alter_timber";

        public async Task Execute(Update update, CancellationToken cancellationToken)
        {
            if (!cancellationToken.IsCancellationRequested)
            {
                if (update != null)
                {
                    var commandParts = update.CallbackQuery.Data.Split(':');


                }
            }
        }
    }
}
