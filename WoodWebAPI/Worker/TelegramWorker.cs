using Telegram.Bot;
using Telegram.Bot.Types;
using WoodWebAPI.Data;
using WoodWebAPI.Data.Entities;

namespace WoodWebAPI.Worker;

public class TelegramWorker : BackgroundService
{
    private readonly ILogger<TelegramWorker> _logger;

    private readonly WoodDBContext _db;

    private readonly IWorkerCreds _telegtamWorkerCreds;

    public static ILogger Logger { get; private set; }
    public static TelegramBotClient? API { get; private set; }

    public TelegramWorker(WoodDBContext context, ILogger<TelegramWorker> logger, IWorkerCreds workerCreds)
    {
        _logger = logger;
        Logger = logger;
        _db = context;
        var mainAdmin = new IsAdmin()
        {
            AdminRole = 1,
            CreatedAt = new DateTime(1997, 04, 10, 10, 51, 54),
            Id = 0,
            TelegramUsername = workerCreds.MainAdmin,
            TelegramId = workerCreds.TelegramId,
        };
        
        if(!_db.IsAdmin.Any(x => x.TelegramUsername == mainAdmin.TelegramUsername && x.TelegramId == mainAdmin.TelegramId))
        {
            _db.IsAdmin.Add(mainAdmin);
        }
       
        _db.SaveChanges();
        _telegtamWorkerCreds = workerCreds;


        //initialisation of telegram bot api
        var botToken = _telegtamWorkerCreds.TelegramToken;
        API = new TelegramBotClient(botToken);
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        //setting telegram webhook

        await API.SetWebhookAsync(_telegtamWorkerCreds.NgrokURL, cancellationToken: cancellationToken);

        // testing telegram connection
        var me = await API.GetMeAsync(cancellationToken: cancellationToken);
        Console.WriteLine($"My name is {me.FirstName}.");

        //  Checking kub records
        ConsumedMethods activateDbConnection = new(_telegtamWorkerCreds);
        await activateDbConnection.GetAsync(cancellationToken);
        var dbContainsKub = await activateDbConnection.GetKubStatus(cancellationToken);
        _logger.LogInformation($"\n{dbContainsKub.Message}\n");

        await API.DeleteMyCommandsAsync(BotCommandScope.Default(), cancellationToken: cancellationToken);

        _logger.LogInformation($"\nРабота на адресе - {_telegtamWorkerCreds.NgrokURL}\n");

        var commands = new List<BotCommand>()
        {
            new()
            {
                Command = "start",
                Description = "В начало"
            },
            new()
            {
                Command = "login",
                Description = "Авторизация"
            },
            new()
            {
                Command = "main",
                Description = "В главное меню"
            },
            new()
            {
                Command = "cancel",
                Description = "Отменить действие"
            },
            new()
            {
                Command = "clear",
                Description = "Удалить сообщения"
            }
        };

        await API.SetMyCommandsAsync(commands, BotCommandScope.Default(), cancellationToken: cancellationToken);
    }
}
