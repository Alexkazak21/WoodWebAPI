using Telegram.Bot;
using Telegram.Bot.Types;
using WoodWebAPI.Data.Entities;

namespace WoodWebAPI.Worker;

public class TelegramWorker : BackgroundService
{
    //private readonly ILogger<TelegramWorker> _logger;

    //private readonly IWorkerCreds _telegtamWorkerCreds;

    //public static TelegramBotClient? API { get; private set; }

    //public static List<IsAdmin> AdminList = new();


    //public TelegramWorker(ILogger<TelegramWorker> logger, IWorkerCreds workerCreds)
    //{
    //    _logger = logger;
    //    Logger = _logger;
    //    BaseUrl = workerCreds.BaseURL;
    //    AdminList.Add(new IsAdmin()
    //    {
    //        AdminRole = 1,
    //        CreatedAt = new DateTime(1997, 04, 10, 10, 51, 54),
    //        Id = 0,
    //        TelegramUsername = workerCreds.MainAdmin,
    //        TelegramId = workerCreds.TelegramId,
    //    });
    //    PriceForM3 = workerCreds.PriceForM3;
    //    PaymentToken = workerCreds.PaymentToken;
    //    MinPrice = workerCreds.MinPrice;
    //    _telegtamWorkerCreds = workerCreds;

    //    //initialisation of telegram bot api
    //    var botToken = _telegtamWorkerCreds.TelegramToken;
    //    API = new TelegramBotClient(botToken);
    //}

    private readonly ILogger<TelegramWorker> _logger;

    private readonly IWorkerCreds _telegtamWorkerCreds;

    public readonly static List<IsAdmin> AdminList = [];

    public static ILogger Logger { get; private set; }
    public static TelegramBotClient? API { get; private set; }
    public TelegramWorker(ILogger<TelegramWorker> logger, IWorkerCreds workerCreds)
    {
        _logger = logger;
        Logger = logger;
        AdminList.Add(new IsAdmin()
        {
            AdminRole = 1,
            CreatedAt = new DateTime(1997, 04, 10, 10, 51, 54),
            Id = 0,
            TelegramUsername = workerCreds.MainAdmin,
            TelegramId = workerCreds.TelegramId,
        });
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