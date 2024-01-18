using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using WoodWebAPI.Data.Models.Order;

namespace WoodWebAPI.Worker.Controller.Commands;

public class DeleteOrderCommand : ICommand
{
    public TelegramBotClient Client => TelegramWorker.API;

    public string Name => "/delete_order";

    public async Task Execute(Update update, CancellationToken cancellationToken)
    {
        if (!cancellationToken.IsCancellationRequested)
        {
            if (update != null)
            {
                int.TryParse(update?.CallbackQuery?.Data?.Substring(update.CallbackQuery.Data.IndexOf(':'), update.CallbackQuery.Data.Length - update.CallbackQuery.Data.IndexOf(':')), out var orderId);



                var userExist = false;

                long chatid = -1;

                if (update.Type == UpdateType.Message)
                {
                    chatid = update.Message.Chat.Id;

                    userExist = await new MainCommand().CheckCustomer(chatid, cancellationToken);

                }
                else if (update.Type == UpdateType.CallbackQuery)
                {
                    chatid = update.CallbackQuery.From.Id;

                    userExist = await new MainCommand().CheckCustomer(chatid, cancellationToken);
                }

                if (userExist && chatid != -1)
                {
                    using (HttpClient httpClient = new HttpClient())
                    {
                        var content = JsonContent.Create(
                            new DeleteOrderDTO()
                            {
                                CustomerTelegramId = chatid.ToString(),
                                OrderId = orderId,
                            });

                        var request = await httpClient.PostAsync("http://localhost:5550/api/Order/DeleteOrder", content, cancellationToken);
                        var response = await request.Content.ReadAsStringAsync(cancellationToken);

                        await Client.SendTextMessageAsync(
                            chatId: chatid,
                            text: $"{response}",
                            replyMarkup: new InlineKeyboardMarkup(
                                                    new[]
                                                    {
                                                    InlineKeyboardButton.WithCallbackData("К заказам","/main"),
                                                    })
                            );
                    }
                }
            }
        }
    }
}
