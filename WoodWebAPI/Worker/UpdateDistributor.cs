using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using WoodWebAPI.Data;
using WoodWebAPI.Worker.Commands;

namespace WoodWebAPI.Worker
{
    public class UpdateDistributor(IWorkerCreds workerCreds, WoodDBContext wood) 
    {
        private Dictionary<long, CommandExecutor> listeners = new();
        private readonly IWorkerCreds _creds = workerCreds;
        private readonly WoodDBContext _dbContext = wood;

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if( update.Message != null && update.CallbackQuery == null)
            {
                long chatId = update.Message.Chat.Id;

                CheckInnerMessageText(update);

                await SendUpdate(chatId, botClient, update, cancellationToken);
            }
            else if(update.CallbackQuery != null)
            {
                long chatId = update.CallbackQuery.From.Id;
                await SendUpdate(chatId, botClient, update, cancellationToken);
            }
            else if(update.Type == UpdateType.PreCheckoutQuery) 
            {
                long chatId = update.PreCheckoutQuery.From.Id;
                await SendUpdate(chatId, botClient, update, cancellationToken);
            }
            
        }

        private async Task SendUpdate(long chatId, ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var listener = listeners.GetValueOrDefault(chatId);
            if (listener is null)
            {
                listener = new(_creds, _dbContext);
                listeners.Add(chatId, listener);
                await listener.HandleUpdateAsync(botClient, update, cancellationToken);
                return;
            }

            await listener.HandleUpdateAsync(botClient, update, cancellationToken);
        }

        private void CheckInnerMessageText(Update update)
        {
            if (update.Message.ReplyToMessage != null && update.Message.ReplyToMessage.Text.StartsWith("Введите длину и диаметр бревна по верхушке"))
            {
                var textParams = update.Message.Text.Split(':');
                var orderId = int.Parse(update.Message.ReplyToMessage.Text.Split('-', StringSplitOptions.TrimEntries)[^1]);
                var diametr = textParams[0];
                var length = textParams[1];
                update.CallbackQuery = new CallbackQuery()
                {
                    Data = $"/add_timber:{orderId}:{diametr}:{length}",
                };
            }
            else if (update.Message.ReplyToMessage != null && update.Message.ReplyToMessage.Text.StartsWith("Введите пароль ОТВЕТОМ на это сообщение"))
            {
                var textParams = update.Message.Text;
                update.CallbackQuery = new CallbackQuery()
                {
                    Data = $"/reg_admin:{textParams}",
                };
            }
            else if(update.Message.Type == MessageType.SuccessfulPayment)
            {
                update.Message.Text = $"/payment";
            }    
        }
    }
}   
