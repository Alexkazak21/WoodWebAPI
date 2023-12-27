using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.BotAPI;
using Telegram.BotAPI.AvailableMethods;
using Telegram.BotAPI.GettingUpdates;

namespace TelegramBotWood.TelegramBot
{
    internal class TelegramBotWorkflow
    {
        public void Run()
        {
            var botToken = "5213388416:AAGSxvbVI9Hd1XiLxBV-cepybFnguamVTPk";
            var api = new BotClient(botToken);
            var consumeMethods = new ConsumeEventSync();

            var me = api.GetMe();
            Console.WriteLine($"My name is {me.FirstName}.");
            var result = consumeMethods.GetAsync();
            Console.WriteLine(result);
        }
    }
}
