using Telegram.BotAPI;
using Telegram.BotAPI.AvailableMethods;
using Telegram.BotAPI.GettingUpdates;
using Telegram.Bot;
using Telegram.Bot.Types;
using WoodWebAPI.Worker.Controller;

namespace WoodWebAPI.Worker;

public class TelegramWorker : BackgroundService
{
    private readonly ILogger<TelegramWorker> _logger;
    private readonly string _telegtamToken;
    private readonly string _ngrokURL;

    public static TelegramBotClient API { get; set; }
    public static ILogger<TelegramWorker> Logger { get; set; }
    public TelegramWorker(ILogger<TelegramWorker> logger, string telegtamToken, string ngrokURL)
    {
        _logger = logger;
        _telegtamToken = telegtamToken;
        _ngrokURL = ngrokURL;
        Logger = _logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        //initialisation of telegram bot api
        var botToken = _telegtamToken;
        API = new TelegramBotClient(botToken);

        //setting telegram webhook

        await API.SetWebhookAsync(_ngrokURL);    

        // testing telegram connection
        var me = await API.GetMeAsync();
        Console.WriteLine($"My name is {me.FirstName}.");





        ////add custom event handler
        //var consumeMethods = new ConsumedMethods();


        //try
        //{
        //    var result = await consumeMethods.GetAsync(stoppingToken);
        //    _logger.LogInformation(result);
        //}
        //catch (Exception ex) { }

        //var updates = await api.GetUpdatesAsync();
        //while(!stoppingToken.IsCancellationRequested)
        //{
        //    if (updates.Any())
        //    {
        //        foreach (var update in updates)
        //        {
        //            var responce = (update.Message.Text == "/hello") ? "Hello world" : "errrors";
        //             await api.SendMessageAsync(update.Message.Chat.Id, responce);
        //            _logger.LogWarning(responce);
        //        }
        //        var offset = updates.Last().UpdateId + 1;
        //        updates = api.GetUpdates(offset);
        //    }
        //    else
        //    {
        //        updates = api.GetUpdates();
        //    }
        //}

        //}
    }
}
