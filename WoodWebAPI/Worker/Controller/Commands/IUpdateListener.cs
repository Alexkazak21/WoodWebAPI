using Telegram.Bot.Types;

namespace WoodWebAPI.Worker.Controller.Commands;

public interface IUpdateListener
{
    public async Task GetUpdateAsync(Update update, CancellationToken cancellationToken) { }
}
