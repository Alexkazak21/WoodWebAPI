using Microsoft.Data.SqlClient;
using WoodWebAPI.Data.Models;

namespace WoodWebAPI.Worker;

public class ConsumedMethods
{
    public async Task<string> GetAsync(CancellationToken cancellationToken = default)
    {
        using (var client = new HttpClient())
        {
            await Task.Delay(1000, cancellationToken);
            var result = await client.PostAsJsonAsync($"{TelegramWorker.BaseUrl}/api/Customer/GetCustomers", new StringContent(""), cancellationToken);
            return await result.Content.ReadAsStringAsync();
        }
    }

    public async Task<ExecResultModel> GetKubStatus(CancellationToken cancellationToken = default)
    {
        var configuration = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json",false)
        .AddJsonFile("appsettings.Development.json", true)
        .AddJsonFile("appsettings.local.json", true)
        .Build();

        var connectionString = configuration.GetConnectionString("ConnStrWood");

        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            await connection.OpenAsync();
            SqlCommand command = new SqlCommand();
            command.CommandText = "select * from EtalonTimberList";
            command.Connection = connection;
            var existCheck = await command.ExecuteReaderAsync();

            if (existCheck.HasRows)
            {
                await connection.CloseAsync();
                return new ExecResultModel { Success = true, Message = "В базе данных присутствуют значения" };
            }
            else 
            {
                await connection.CloseAsync();
                await connection.OpenAsync();
                var pathToWood = "./fullKub.sql";
                command.CommandText = File.ReadAllText(pathToWood);
                command.Connection = connection;
                await command.ExecuteNonQueryAsync();
                await connection.CloseAsync();

                return new ExecResultModel { Success = true, Message = "Данные в базу успешно добавлены можно работать" };
            }
        }
    }
}
