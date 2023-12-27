using Telegram.BotAPI;
using Telegram.BotAPI.AvailableMethods;
using Telegram.BotAPI.GettingUpdates;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace WoodWebAPI.Worker;

public class TelegramWorker : BackgroundService
{
    private readonly ILogger<TelegramWorker> _logger;
    private readonly string _telegtamToken;

    public TelegramWorker(ILogger<TelegramWorker> logger, string telegtamToken)
    {
        _logger = logger;
        _telegtamToken = telegtamToken;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var botToken = _telegtamToken;

        var api = new BotClient(botToken);

        var consumeMethods = new ConsumedMethods();

        var me = await api.GetMeAsync();
        Console.WriteLine($"My name is {me.FirstName}.");

        try
        {
            var result = await consumeMethods.GetAsync(stoppingToken);
            _logger.LogInformation(result);
        }
        catch (Exception ex) { }

        var updates = await api.GetUpdatesAsync();
        while(!stoppingToken.IsCancellationRequested)
        {
            if (updates.Any())
            {
                foreach (var update in updates)
                {
                    var responce = (update.Message.Text == "/hello") ? "Hello world" : "errrors";
                     await api.SendMessageAsync(update.Message.Chat.Id, responce);
                    _logger.LogWarning(responce);
                }
                var offset = updates.Last().UpdateId + 1;
                updates = api.GetUpdates(offset);
            }
            else
            {
                updates = api.GetUpdates();
            }
        }

        //}
    }
}
