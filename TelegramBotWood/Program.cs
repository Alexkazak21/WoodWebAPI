using TelegramBotWood.TelegramBot;

namespace TelegramBotWood
{
    internal class Program
    {
        static void Main(string[] args)
        {
            TelegramBotWorkflow tbot = new TelegramBotWorkflow();
            tbot.Run();
        }
    }
}
