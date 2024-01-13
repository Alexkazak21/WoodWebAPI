using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bots.Requests;

namespace WoodWebAPI.Worker.Controller.Commands;


public class CommandExecutor : IUpdateHandler
{
    public async Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if(update.Message != null && update.CallbackQuery == null) // ожидается получнени только сообщения
        {
            Message msg = update.Message;
            if (msg.Text == null) //такое бывает, во избежании ошибок делаем проверку
                return;
            bool macth = false;
            foreach (var command in commands)
            {
                if (command.Name == msg.Text)
                {
                    macth = true;
                    await command.Execute(update, cancellationToken);
                }
            }

            if (!macth)
            {
                var prevMessage = botClient;
            }
        }
        else if (update.Message != null && update.CallbackQuery != null) // ожидаем сообщение к которому привязан текст через CallbackQuery
        {
            CallbackQuery query = update.CallbackQuery;
            if (query.Data == null) //такое бывает, во избежании ошибок делаем проверку
                return;

            foreach (var command in commands)
            {
                if (command.Name == query.Data)
                {
                    await command.Execute(update, cancellationToken);
                }
            }
        }
        else if (update.Message == null && update.CallbackQuery != null) // ожидаем на входе только нажатие на кнопку
        { 
            CallbackQuery query = update.CallbackQuery;
            if (query.Data == null) //такое бывает, во избежании ошибок делаем проверку
                return;

            foreach (var command in commands)
            {
                if (command.Name == query.Data)
                {
                    await command.Execute(update, cancellationToken);
                }
            }
        }        
    }


    private List<ICommand> commands;
    private IListener? listener = null;

    public CommandExecutor()
    {
        commands =  new List<ICommand>() //GetCommands();
        {
            new StartCommand(),
            new CancelCommand(),
            new SignUpCommand(this),
        };
    }

    public async Task GetUpdate(Update update,CancellationToken cancellationToken)
    {
        if (listener == null)
        {
            await ExecuteCommand(update, cancellationToken);
        }
        else
        {
            await listener.GetUpdate(update,cancellationToken);
        }
    }

    private async Task ExecuteCommand(Update update,CancellationToken cancellationToken)
    {
        Message msg = update.Message;
        foreach (var command in commands)
        {
            if (command.Name == msg.Text)
            {
                await command.Execute(update,cancellationToken);
            }
        }
    }

    public void StartListen(IListener newListener)
    {
        listener = newListener;
    }

    public void StopListen()
    {
        listener = null;
    }

    private List<ICommand> GetCommands()
    {
        var types = AppDomain
                  .CurrentDomain
                  .GetAssemblies()
                  .SelectMany(assembly => assembly.GetTypes())
                  .Where(type => typeof(ICommand).IsAssignableFrom(type))
                  .Where(type => type.IsClass);

        List<ICommand> commands = new List<ICommand>();
        foreach (var type in types)
        {
            ICommand? command;
            if (typeof(IListener).IsAssignableFrom(type))
            {
                command = Activator.CreateInstance(type, this) as ICommand;
            }
            else
            {
                command = Activator.CreateInstance(type) as ICommand;
            }

            if (command != null)
            {
                commands.Add(command);
            }
        }
        return commands;
    }
}
