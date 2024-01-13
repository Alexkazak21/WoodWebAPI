﻿using Telegram.Bot;
using Telegram.Bot.Types;

namespace WoodWebAPI.Worker.Controller.Commands;

public class CancelCommand : ICommand
{
    public TelegramBotClient Client => TelegramWorker.API;
    private string _token = TelegramWorker.Token;
    public string Name => "/cancel";
    public async Task Execute(Update update, CancellationToken cancellationToken)
    {
        if (!cancellationToken.IsCancellationRequested)
        {
            long chatId = update.Message.Chat.Id;
            var messageId = update.Message.MessageId;
            await Client.SendTextMessageAsync(chatId, "Операция отменена. Вы вернётесь в начало");
        }
        else
        {
            return;
        }
    }
}