using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using WoodWebAPI.Data.Models.Customer;

namespace WoodWebAPI.Worker.Controller.Commands;

public class OrderManageCommand : ICommand
{
    public TelegramBotClient Client => TelegramWorker.API;

    public string Name => "/order_manage";

    public async Task Execute(Update update, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested) { return; }

        if(update != null) 
        {
            var chatId = 0l;
            string[]? commandParts = null;
            var messageid = -1;
            if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message)
            {
                chatId = update.Message.From.Id;
            }
            else if(update.Type == Telegram.Bot.Types.Enums.UpdateType.CallbackQuery) 
            {
                chatId = update.CallbackQuery.From.Id;
                commandParts = update.CallbackQuery.Data.Split(":");
                messageid = update.CallbackQuery.Message.MessageId;
            }

            if(commandParts != null && commandParts.Length > 1) 
            {
                if (commandParts[1] == "all")
                {
                    if (commandParts[2] != null)
                    {
                        await ShowAllOrders(update, int.Parse(commandParts[2]));
                    }
                    else
                    {
                        await ShowAllOrders(update);
                    }
                }
                else if(commandParts[1] == "true")
                {
                    if (commandParts[2] != null)
                    {
                        await ShowAllOrders(update, int.Parse(commandParts[2]));
                    }
                    else
                    {
                        await ShowVerifiedOrders(update);
                    }
                }
                else if (commandParts[1] == "false")
                {
                    if (commandParts[2] != null)
                    {
                        await ShowAllOrders(update, int.Parse(commandParts[2]));
                    }
                    else
                    {
                        await ShowNotVerifiedOrders(update);
                    }                    
                }
                else
                {
                    TelegramWorker.Logger.LogWarning("Wrong param while proccessing order manage command");
                }
            }
            else
            {
                InlineKeyboardMarkup replyMarkup = new InlineKeyboardMarkup(
                    new[]
                    {
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("Весь список заказов","/order_manage:all")
                        },
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("Заказы в работе","/order_manage:true")
                        },
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("Ожидают подтверждение","/order_manage:false")
                        },
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("Главное меню","/main"),
                        }
                    });
                if (messageid > 0)
                {
                    await Client.EditMessageTextAsync(
                        chatId: chatId,
                        messageId: messageid,
                        text: "Выберите нужное действие",
                        replyMarkup: replyMarkup,
                        cancellationToken: cancellationToken
                    );
                }
                else
                {
                    await Client.SendTextMessageAsync(
                        chatId: chatId,
                        text: "Выберите нужное действие",
                        replyMarkup: replyMarkup,
                        cancellationToken: cancellationToken
                    );
                }
                
            }
        }
    }

    private async Task ShowAllOrders(Update update, int? orderId = null, CancellationToken cancellationToken = default)
    {
        if( cancellationToken.IsCancellationRequested ) return;

        if( update != null )
        {
            var customerId = -1l;

            bool approved = false;

            if( update.Type == Telegram.Bot.Types.Enums.UpdateType.Message ) 
            {
                customerId = update.Message.From.Id;
                approved = await new CommonChecks().CheckCustomer(customerId, cancellationToken);
            }
            else if( update.Type == Telegram.Bot.Types.Enums.UpdateType.CallbackQuery )
            {
                customerId = update.CallbackQuery.From.Id;
                approved = true;
            }

            var chatId = customerId;

            if(approved)
            {
                var customersList = new List<GetCustomerModel>();
            }
        }


    }

    private async Task ShowVerifiedOrders(Update update, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested) return;
    }

    private async Task ShowNotVerifiedOrders(Update update, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested) return;
    }
}
