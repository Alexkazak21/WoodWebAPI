using Microsoft.Data.SqlClient;
using System.Runtime.CompilerServices;
using WoodWebAPI.Data;
using WoodWebAPI.Data.Entities;
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
        .AddJsonFile("appsettings.json",false)
        .AddJsonFile("appsettings.Development.json", true)
        .AddJsonFile("appsettings.local.json", true)
        .Build();

        var connectionString = configuration.GetConnectionString("ConnStrWood");

        using (SqlConnection connection = new SqlConnection(connectionString))
        {
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
                return new ExecResultModel { Success = true, Message = "В базе данных присутствуют значения" };
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

    public static  bool IsValidTransition(OrderStatus currentStatus, OrderStatus newStatus)
    {
        return currentStatus switch
        {
            OrderStatus.NewOrder => newStatus == OrderStatus.Canceled || newStatus == OrderStatus.Verivied,
            OrderStatus.Canceled => newStatus == OrderStatus.Archived,
            OrderStatus.Verivied => newStatus == OrderStatus.Completed || newStatus == OrderStatus.CanceledByAdmin,
            OrderStatus.CanceledByAdmin => newStatus == OrderStatus.Archived,
            OrderStatus.Completed => newStatus == OrderStatus.Paid,
            OrderStatus.Paid => newStatus == OrderStatus.Archived,
            _ => false,
        };
    }
}
