﻿using Newtonsoft.Json;
using System.Globalization;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using WoodWebAPI.Data.Models;
using WoodWebAPI.Data.Models.OrderPosition;

namespace WoodWebAPI.Worker.Commands;

public class AddOrderPositionCommand(IWorkerCreds workerCreds) : ICommand
{
    private readonly IWorkerCreds _workerCreds = workerCreds;
    public TelegramBotClient Client => TelegramWorker.API;

    public string Name => "/add_timber";

    public async Task Execute(Update update, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested || update == null)
        {
            return;
        }

        var userExist = false;

        long chatid = -1;

        int orderId = -1;

        int messageId = -1;

        decimal diameter = -1m;

        decimal timberLength = -1.0m;

        var comandParts = update?.CallbackQuery?.Data?.Split(':');

        if (update.Type == UpdateType.Message)
        {
            chatid = update.Message.Chat.Id;
            messageId = update.Message.MessageId;
            userExist = await new CommonChecks(_workerCreds).CheckCustomer(chatid, cancellationToken);
        }
        else if (update.Type == UpdateType.CallbackQuery)
        {
            chatid = update.CallbackQuery.From.Id;
            messageId = update.CallbackQuery.Message.MessageId;
            userExist = await new CommonChecks(_workerCreds).CheckCustomer(chatid, cancellationToken);
        }


        if (comandParts.Length == 2)
        {
            try
            {
                int.TryParse(comandParts[1], out orderId);
            }
            catch (Exception ex)
            {
                TelegramWorker.Logger.LogError("Can`t get order id while executing add timber command");
            }

            await Client.EditMessageTextAsync(
                        chatId: chatid,
                        text: "Введите длину и диаметр бревна по верхушке(конец с меньшим диаметром) в формате" +
                        "\n        диаметр:длина" +
                        "\nДиаметр в сантиметрах, а длина в метрах с точностью до 1см" +
                        "\n\tНапример  56:1.58" +
                        "\n\nВНИМАНИЕ! Если введённые вами значения не соотвертсвуют ГОСТ 2708-75, " +
                        "то они будут округлены до соответствующих значений" +
                        "\n\nОтправьте как ОТВЕТ на это сообщение" +
                        $"\nЗаказ - {orderId}",
                        messageId: messageId,
                        replyMarkup: null,
                        cancellationToken: cancellationToken);
        }

        if (comandParts.Length == 4)
        {
            try
            {
                _ = int.TryParse(comandParts[1], out orderId);
                _ = decimal.TryParse(comandParts[2], out diameter);
                _ = decimal.TryParse(comandParts[3], CultureInfo.InvariantCulture, out timberLength);

                if (userExist && chatid != -1 && orderId != -1 && diameter != -1 && timberLength != -1)
                {
                    var orders = await new CommonChecks(_workerCreds).CheckOrdersOfCustomer(chatid, cancellationToken);

                    if (orders != null)
                    {
                        foreach (var order in orders)
                        {
                            if (order.Id == orderId)
                            {
                                var volume = 0.0;
                                using HttpClient httpClient = new();

                                AddOrderPositionDTO addTimber = new()
                                {
                                    OrderId = orderId,
                                    Length = timberLength,
                                    Diameter = diameter,
                                };

                                var content = JsonContent.Create(addTimber);

                                var request = await httpClient.PostAsync($"{_workerCreds.BaseURL}/api/OrderPosition/AddOrderPositionToOrder", content, cancellationToken);

                                var responce = await request.Content.ReadAsStringAsync(cancellationToken);
                                var result = JsonConvert.DeserializeObject<ExecResultModel>(responce);
                                if (result != null && result.Success)
                                {
                                    await Client.SendTextMessageAsync(
                                        chatId: chatid,
                                        text: $"{result.Message}",
                                        replyMarkup: new InlineKeyboardMarkup(
                                           InlineKeyboardButton.WithCallbackData("Вернуться к заказу", $"/show_order:{orderId}")),
                                        cancellationToken: cancellationToken);
                                }
                                else if (result != null && !result.Success)
                                {
                                    await Client.SendTextMessageAsync(
                                        chatId: chatid,
                                        text: $"{result.Message}",
                                        replyMarkup: new InlineKeyboardMarkup(
                                           InlineKeyboardButton.WithCallbackData("Вернуться к заказу", $"/show_order:{orderId}")),
                                        cancellationToken: cancellationToken);
                                }
                                else
                                {
                                    await Client.SendTextMessageAsync(
                                        chatId: chatid,
                                        text: $"Что-то пошло не так, повторите попытку позже",
                                        replyMarkup: new InlineKeyboardMarkup(
                                           InlineKeyboardButton.WithCallbackData("Вернуться к заказу", $"/show_order:{orderId}")),
                                        cancellationToken: cancellationToken);

                                    string message = $"{request.StatusCode}\t{request.RequestMessage.RequestUri}";

                                    TelegramWorker.Logger.LogWarning(message);
                                }

                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                TelegramWorker.Logger.LogError("Wrong data when parsing information about adding timber to order");
            }
        }
    }
}
