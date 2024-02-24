﻿using Newtonsoft.Json;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.Payments;
using WoodWebAPI.Data.Models;
using WoodWebAPI.Data.Models.Order;
using WoodWebAPI.Data.Entities;

namespace WoodWebAPI.Worker.Controller.Commands;

public class PaymentCommand : ICommand
{
    public TelegramBotClient Client => TelegramWorker.API;

    public string Name => "/payment";

    public async Task Execute(Update update, CancellationToken cancellationToken)
    {
        if (update != null)
        {
            if (update.CallbackQuery != null)
            {
                var commandParts = update.CallbackQuery.Data.Split(':');
                var chatId = long.Parse(commandParts[1]);

                if (update.PreCheckoutQuery != null)
                {
                    var telegramId = long.Parse(commandParts[1]);
                    var orderId = int.Parse(update.PreCheckoutQuery.InvoicePayload);
                    var orders = await new CommonChecks().CheckOrdersOfCustomer(telegramId, cancellationToken);

                    if (orders.Where(x => x.Id == orderId && x.Status == OrderStatus.Completed).First() != null)
                    {
                        var precheckId = update.PreCheckoutQuery.Id;
                        await Client.AnswerPreCheckoutQueryAsync(
                            preCheckoutQueryId: precheckId);

                        return;
                    }
                }

                var totalSumToPay = decimal.Parse(commandParts[2]) > TelegramWorker.MinPrice ? decimal.Parse(commandParts[2]) : TelegramWorker.MinPrice;

                await Client.SendInvoiceAsync(
                    chatId: chatId,
                    title: $"Распил заказа № {commandParts[3]}",
                    description: "Заказ на распил древисины завершён в полном объёме." +
                    "\nПродолжая ВЫ соглашаетесь с условиями оплаты",
                    payload: $"{commandParts[3]}",
                    providerToken: TelegramWorker.PaymentToken,
                    currency: "BYN",
                    prices: new[]
                    {
                        new LabeledPrice("сумма",(int)(totalSumToPay * 100))
                    },
                    photoUrl: "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSom0yXqOCEoHjbbhYBeDoAJrCL0X1tfvki6Q&usqp=CAU"
                    );
            }
            else if (update.Message != null && update.Type == UpdateType.Message && update.Message.SuccessfulPayment != null)
            {
                var telegramId = update.Message.From.Id;
                var orderId = int.Parse(update.Message.SuccessfulPayment.InvoicePayload);
                var orders = await new CommonChecks().CheckOrdersOfCustomer(telegramId, cancellationToken);

                if (orders.Where(x => x.Id == orderId && x.Status == OrderStatus.Completed).First() != null)
                {

                    using (HttpClient httpClient = new HttpClient())
                    {
                        var content = JsonContent.Create(
                            new VerifyOrderDTO()
                            {
                                OrderId = orderId,
                            });

                        var request = await httpClient.PostAsync($"{TelegramWorker.BaseUrl}/api/Order/PaidSuccessfull", content);
                        var responce = JsonConvert.DeserializeObject<ExecResultModel>(await request.Content.ReadAsStringAsync());
                        if (!responce.Success)
                        {
                            TelegramWorker.Logger.LogError("Неудалось сохранить оплату по причине\n" + responce.Message);
                        }

                    }

                    var configuration = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json", false)
                    .AddJsonFile("appsettings.Development.json", true)
                    .AddJsonFile("appsettings.local.json", true)
                    .Build();

                    var pathToPaymentsLogFile = "./" + configuration.GetValue<string>("PaymentsLogFile");
                    StringBuilder stringBuilder = new StringBuilder();
                    stringBuilder.AppendLine("\n========================================================================================");
                    stringBuilder.AppendLine($"Поступила оплата от {update.Message.From.FirstName} с ID={update.Message.From.Id}");
                    stringBuilder.AppendLine($"Оплата в размере {(decimal)update.Message.SuccessfulPayment.TotalAmount / 100} BYN");
                    stringBuilder.AppendLine($"Платёжный идентификатор в Telegram: {update.Message.SuccessfulPayment.TelegramPaymentChargeId}");
                    stringBuilder.AppendLine($"Платёжный идентификатор поставшика: {update.Message.SuccessfulPayment.ProviderPaymentChargeId}");
                    stringBuilder.AppendLine("==========================================================================================");

                    using (StreamWriter writer = new StreamWriter(pathToPaymentsLogFile))
                    {
                        await writer.WriteAsync(stringBuilder);
                    }
                    return;
                }
            }
        }
    }
}