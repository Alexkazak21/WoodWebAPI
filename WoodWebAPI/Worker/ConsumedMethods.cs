using Microsoft.Data.SqlClient;
using WoodWebAPI.Data.Models;

namespace WoodWebAPI.Worker;

public class ConsumedMethods(IWorkerCreds workerCreds)
{
    private readonly IWorkerCreds _creds = workerCreds;

    public async Task<string> GetAsync(CancellationToken cancellationToken = default)
    {
        using var client = new HttpClient();
        await Task.Delay(1000, cancellationToken);
        var result = await client.PostAsJsonAsync($"{_creds.BaseURL}/api/Customer/GetCustomers", new StringContent(""), cancellationToken);
        return await result.Content.ReadAsStringAsync(cancellationToken);
    }

    public async Task<ExecResultModel> GetKubStatus(CancellationToken cancellationToken = default)
    {
        var configuration = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json", false)
        .AddJsonFile("appsettings.Development.json", true)
        .AddJsonFile("appsettings.local.json", true)
        .Build();

        var connectionString = configuration.GetConnectionString("ConnStrWood");

        using SqlConnection connection = new(connectionString);

        await connection.OpenAsync(cancellationToken);
        SqlCommand command = new()
        {
            CommandText = $"select * from EtalonTimberList",
            Connection = connection
        };
        var existCheck = await command.ExecuteReaderAsync(cancellationToken);

        if (existCheck.HasRows)
        {
            await connection.CloseAsync();
            return new ExecResultModel { Success = true, Message = "В базе данных присутствуют значения согласно ГОСТ" };
        }
        else
        {
            await connection.CloseAsync();
            await connection.OpenAsync(cancellationToken);
            var pathToWood = "./fullKub.sql";
            command.CommandText = File.ReadAllText(pathToWood);
            command.Connection = connection;
            await command.ExecuteNonQueryAsync(cancellationToken);
            await connection.CloseAsync();

            return new ExecResultModel { Success = true, Message = "Данные в базу успешно добавлены можно работать" };
        }

    }
}