using Microsoft.AspNetCore.Mvc;
using Telegram.Bot;
using Telegram.Bot.Types;
using WoodWebAPI.Worker.Controller.Commands;

namespace WoodWebAPI.Worker.Controller
{
    [Route("/")]
    [ApiController]
    public class BotController(ILogger<BotController> logger,IWorkerCreds workerCreds) : ControllerBase
    {
        private readonly TelegramBotClient _bot = TelegramWorker.API;
        private readonly ILogger<BotController>? _logger = logger;
        private readonly UpdateDistributor<CommandExecutor> _distributor = new(workerCreds);

        [HttpPost]
        public async void Post(Update update, CancellationToken cancellationToken)
        {
            if (update.Message != null) // проверка на наличие текстового сообщения
            {
                _logger.LogInformation(DateTime.UtcNow + " +3\n" + update.Message.Chat.Id + "\t" + update.Message.Text);                

                await _distributor.HandleUpdateAsync(_bot, update, cancellationToken);
            }
            else if (update.CallbackQuery != null && update.Message == null)
            {
                _logger.LogInformation(DateTime.UtcNow +" +3" + $"\n\tCallback Query from {update.CallbackQuery.From.Id}"
                    + $"\n\tWith text:   {update.CallbackQuery.Data}");

                await _distributor.HandleUpdateAsync(_bot, update, cancellationToken);
            }
            else if(update.PreCheckoutQuery != null)
            {
                _logger.LogInformation(DateTime.UtcNow + " +3" + $"\n\tPayment Query with text {update.PreCheckoutQuery.OrderInfo}"
                   + $"\n\tWith ammount:   {update.PreCheckoutQuery.TotalAmount/100}");

                await _distributor.HandleUpdateAsync(_bot, update, cancellationToken);
            }
            return;
        }

        [HttpGet]
        public string Get()
        {
            //Здесь мы пишем, что будет видно если зайти на адрес,
            //указаную в ngrok и launchSettings
            return "Telegram bot was started";
        }
    }
}
