﻿using Telegram.Bot;
using Telegram.Bot.Types;

namespace WoodWebAPI.Worker.Controller.Commands;

public interface ICommand
{
    public TelegramBotClient Client { get; }

    public string Name { get; }

    public async Task Execute(Update update, CancellationToken cancellationToken) { }
}
