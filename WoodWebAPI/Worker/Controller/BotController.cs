using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot;
using Telegram.Bot.Types;
using WoodWebAPI.Worker.Controller.Commands;

namespace WoodWebAPI.Worker.Controller
{
    [Route("/")]
    [ApiController]
    public class BotController : ControllerBase
    {
        private readonly TelegramBotClient _bot = TelegramWorker.API;   
        private readonly ILogger<TelegramWorker> _logger = TelegramWorker.Logger;
        private readonly UpdateDistributor<CommandExecutor> _distributor = new UpdateDistributor<CommandExecutor>();
       
        [HttpPost]
        public async void Post(Update update, CancellationToken cancellationToken)
        {
            if (update.Message == null) //и такое тоже бывает, делаем проверку
                return;

            _logger.LogInformation(update.Message.Chat.Id + "\t" + update.Message.Text);

            await _distributor.GetUpdateAsync(update, cancellationToken);

            
            //await _bot.SendTextMessageAsync(update.Message.Chat.Id, "hello");
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
