namespace WoodWebAPI.Worker;

public class ConsumedMethods
{
    public async Task<string> GetAsync(CancellationToken cancellationToken = default)
    {
        using (var client = new HttpClient())
        {
            await Task.Delay(1000,cancellationToken);
            //var content = new StringContent("");
            var result = await client.PostAsJsonAsync("http://localhost:5550/api/Customer/GetCustomers",new StringContent(""),cancellationToken);
            return await result.Content.ReadAsStringAsync();
        }
    }
}
