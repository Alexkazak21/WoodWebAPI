using Telegram.Bot;
using Telegram.Bot.Types;

namespace WoodWebAPI.Worker.Commands;

public interface ICommand
{
    public TelegramBotClient Client { get; }

    public string Name { get; }

    public Task Execute(Update update, CancellationToken cancellationToken);
}