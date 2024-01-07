using Telegram.Bot.Types;

namespace WoodWebAPI.Worker.Controller.Commands;


public class CommandExecutor : IUpdateListener
{
    public async Task GetUpdateAsync(Update update, CancellationToken cancellationToken)
    {
        Message msg = update.Message;
        if (msg.Text == null) //такое бывает, во избежании ошибок делаем проверку
            return;

        foreach (var command in commands)
        {
            if (command.Name == msg.Text)
            {
                await command.Execute(update,cancellationToken);
            }
        }
    }

    private List<ICommand> commands;

    public CommandExecutor()
    {
        commands = new List<ICommand>()
        {
            new StartCommand()
        };
    }
}
