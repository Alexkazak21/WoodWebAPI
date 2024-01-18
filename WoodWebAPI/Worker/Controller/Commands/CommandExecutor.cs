﻿using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;

namespace WoodWebAPI.Worker.Controller.Commands;


public class CommandExecutor : IUpdateHandler
{
    public async Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if(update.Message != null && update.CallbackQuery == null) // ожидается получнени только сообщения
        {
            Message msg = update.Message;
            if (msg.Text == null) //такое бывает, во избежании ошибок делаем проверку
                return;
            bool macth = false;
            foreach (var command in commands)
            {
                if (command.Name == msg.Text)
                {
                    macth = true;
                    await command.Execute(update, cancellationToken);
                }
            }

            if (!macth)
            {
                var prevMessage = botClient;
            }
        }
        else if (update.Message != null && update.CallbackQuery != null) // ожидаем сообщение к которому привязан текст через CallbackQuery
        {
            CallbackQuery query = update.CallbackQuery;
            if (query.Data == null) //такое бывает, во избежании ошибок делаем проверку
                return;

            foreach (var command in commands)
            {
                if (command.Name == query.Data)
                {
                    await command.Execute(update, cancellationToken);
                }
            }
        }
        else if (update.Message == null && update.CallbackQuery != null) // ожидаем на входе только нажатие на кнопку
        { 
            CallbackQuery query = update.CallbackQuery;
            if (query.Data == null) //такое бывает, во избежании ошибок делаем проверку
                return;

            foreach (var command in commands)
            {
                if (command.Name == query.Data)
                {
                    await command.Execute(update, cancellationToken);
                }
            }
        }        
    }


    private List<ICommand> commands;

    public CommandExecutor()
    {
        commands =  new List<ICommand>()
        {
            new StartCommand(),
            new CancelCommand(),
            new SignUpCommand(),
            new LoginCommand(),
            new MainCommand(),
            new NewOrderCommand(),
            new DeleteOrderCommand(),
        };
    }
}