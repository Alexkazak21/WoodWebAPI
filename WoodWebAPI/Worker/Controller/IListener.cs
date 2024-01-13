using Telegram.Bot.Types;
using WoodWebAPI.Worker.Controller.Commands;

namespace WoodWebAPI.Worker.Controller;

public interface IListener
{
    public async Task GetUpdate(Update update, CancellationToken cancellationToken) { }

    public CommandExecutor Executor { get; }
}
