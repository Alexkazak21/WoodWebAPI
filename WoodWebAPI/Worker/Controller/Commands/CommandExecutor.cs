using System.Reflection;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;

namespace WoodWebAPI.Worker.Controller.Commands;


public class CommandExecutor : IUpdateHandler
{
    public async Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if(update.Message != null && update.CallbackQuery == null) // ожидается получнение только сообщения
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
            CallbackQuery query = new CallbackQuery();
            query.Data = update.CallbackQuery.Data;

            if (query.Data == null) //такое бывает, во избежании ошибок делаем проверку
                return;

            if (query.Data.IndexOf(':') > 0)
            {
                query.Data = query.Data.Split(':')[0];
            }

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
            CallbackQuery query = new CallbackQuery();
            query.Data = update.CallbackQuery.Data;
            if (query.Data == null) //такое бывает, во избежании ошибок делаем проверку
                return;

            if (query.Data.IndexOf(':') > 0)
            {
                query.Data = query.Data.Substring(0, query.Data.IndexOf(':'));
            }

            foreach (var command in commands)
            {
                if (command.Name == query.Data)
                {
                    await command.Execute(update, cancellationToken);
                }
            }
        }
        else if(update.Type == Telegram.Bot.Types.Enums.UpdateType.PreCheckoutQuery)
        {
            update.CallbackQuery = new CallbackQuery()
            {
                From = update.PreCheckoutQuery.From,
                Data = $"/payment:{update.PreCheckoutQuery.From.Id}",
            };

            foreach (var command in commands)
            {
                if (command.Name == update.CallbackQuery.Data.Split(':')[0])
                {
                    await command.Execute(update, cancellationToken);
                }
            }
        }
    }


    private List<ICommand> commands;

    public CommandExecutor()
    {
        //var type = AppDomain.CurrentDomain.GetAssemblies()
        //    .SelectMany(x => x.GetTypes())
        //    .Where(x => x.IsClass)
        //    .Where(x => typeof(ICommand).IsAssignableFrom(x))
        //    .Select(x => x.FullName).ToList();

        commands = new List<ICommand>()
        {

            new StartCommand(),
            new CancelCommand(),
            new SignUpCommand(),
            new LoginCommand(),
            new MainCommand(),
            new AddOrderCommand(),
            new DeleteOrderCommand(),
            new ShowOrderCommand(),
            new AddOrderPositionCommand(),
            new ClearCommand(),
            new AlterOrderPositionCommand(),
            new RegAdminCommand(),
            new OrderManageCommand(),
            new AdminManageCommand(),
            new PaymentCommand(),
        };
    }
}