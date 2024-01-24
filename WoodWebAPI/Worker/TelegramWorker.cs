using Telegram.Bot;
using Telegram.Bot.Types;

namespace WoodWebAPI.Worker;

public class TelegramWorker : BackgroundService
{
    private readonly ILogger<TelegramWorker> _logger;

    private readonly string _ngrokURL;

    private readonly string _telegtamToken;


    public static string BaseUrl { get; private set; }
    public static TelegramBotClient? API { get; set; }
    public static ILogger<TelegramWorker>? Logger { get; set; }

    public TelegramWorker(ILogger<TelegramWorker> logger, string telegtamToken, string ngrokURL, string baseUrl)
    {
        _logger = logger;
        _telegtamToken = telegtamToken;
        _ngrokURL = ngrokURL;
        Logger = _logger;
        BaseUrl = baseUrl;
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
        await API.DeleteMyCommandsAsync();

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

        await API.SetMyCommandsAsync(commands);
    }
}
