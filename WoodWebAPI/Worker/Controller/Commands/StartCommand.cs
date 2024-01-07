using Telegram.Bot;
using Telegram.Bot.Types;

namespace WoodWebAPI.Worker.Controller.Commands;

public class StartCommand : ICommand
{
    public TelegramBotClient Client => TelegramWorker.API;

    public string Name => "/start";

    public async Task Execute(Update update,CancellationToken cancellationToken)
    {
        long chatId = update.Message.Chat.Id;
        await Client.SendTextMessageAsync(chatId, "Привет! " + update.Message.Chat.FirstName);
    }
}
