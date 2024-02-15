using Telegram.Bot;
using Telegram.Bot.Types;

namespace WoodWebAPI.Worker.Controller.Commands;

public class PaymentCommand : ICommand
{
    public TelegramBotClient Client => TelegramWorker.API;

    public string Name => "/payment";

    public async Task Execute(Update update, CancellationToken cancellationToken)
    {
        if(update != null)
        {
            if (update.CallbackQuery != null)
            {
                var commandParts = update.CallbackQuery.Data.Split(':');
                var chatId = long.Parse(commandParts[1]);
                var totalSumToPay = decimal.Parse(commandParts[2]);


            }
        }
    }
}
