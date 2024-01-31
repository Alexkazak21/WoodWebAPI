using Telegram.Bot;
using Telegram.Bot.Types;
using WoodWebAPI.Data.Entities;

namespace WoodWebAPI.Worker;

public class TelegramWorker : BackgroundService
{
    private readonly ILogger<TelegramWorker> _logger;

    private readonly string _ngrokURL;

    private readonly string _telegtamToken;


    public static string BaseUrl { get; private set; }
    public static TelegramBotClient? API { get; set; }
    public static ILogger<TelegramWorker>? Logger { get; set; }

    public static readonly List<IsAdmin> AdminList = new List<IsAdmin>();

    public TelegramWorker(ILogger<TelegramWorker> logger, TelegtamWorkerCreds workerCreds)
    {
        _logger = logger;
        _telegtamToken = workerCreds.TelegramToken;
        _ngrokURL = workerCreds.NgrokURL;
        Logger = _logger;
        BaseUrl = workerCreds.BaseURL;
        AdminList.Add(new IsAdmin()
        {
            AdminRole = 1,
            CreatedAt = new DateTime(1997,04,10, 10, 51, 54),
            Id = 0,
            TelegramUsername = workerCreds.MainAdmin,
            TelegramId = workerCreds.TelegramId,
        });
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        //initialisation of telegram bot api
        var botToken = _telegtamToken;
        API = new TelegramBotClient(botToken);

        //setting telegram webhook

        await API.SetWebhookAsync(_ngrokURL);

        // testing telegram connection
        var me = await API.GetMeAsync();
        Console.WriteLine($"My name is {me.FirstName}.");

        ConsumedMethods activateDbConnection = new ConsumedMethods();
        await activateDbConnection.GetAsync(cancellationToken);
        await API.DeleteMyCommandsAsync(BotCommandScope.Default());

        var commands = new List<BotCommand>()
        {
            new BotCommand()
            {
                Command = "start",
                Description = "В начало"
            },
            new BotCommand()
            {
                Command = "login",
                Description = "Авторизация"
            },
            new BotCommand()
            {
                Command = "main",
                Description = "В главное меню"
            },
            new BotCommand()
            {
                Command = "cancel",
                Description = "Отменить действие"
            },
            new BotCommand()
            {
                Command = "clear",
                Description = "Удалить сообщения"
            }
        };

        await API.SetMyCommandsAsync(commands,BotCommandScope.Default());
    }
}

public class TelegtamWorkerCreds
{
    private readonly string _telegtamToken;
    private readonly string _ngrokURL;
    private readonly string _baseUrl;
    private readonly string _mainAdmin;
    private readonly string? _telegramId;

    public string TelegramToken { get => _telegtamToken; }
    public string NgrokURL { get => _ngrokURL; }
    public string BaseURL { get => _baseUrl; }
    public string MainAdmin { get => _mainAdmin; }
    public string? TelegramId { get => _telegramId; }

    public TelegtamWorkerCreds(string telegramToken, string ngrokURL, string baseUrl, string mainAdmin, string? telegramId)
    {
        _telegramId = telegramId;
        _baseUrl = baseUrl;
        _mainAdmin = mainAdmin;
        _telegtamToken = telegramToken;
        _ngrokURL = ngrokURL;
    }

}
