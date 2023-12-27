namespace TelegramBotWood.TelegramBot
{
    public class ConsumeEventSync
    {
        public string GetAsync()
        {
            using (var client = new HttpClient())
            { 
                //var content = new StringContent("");
                var result = client.GetStringAsync("http://localhost:5550/api/Customer/GetCustomers");
                var res = result.Result;
                return res;
            }
        }
    }
}
