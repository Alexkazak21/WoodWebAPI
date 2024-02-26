﻿using Newtonsoft.Json;
using WoodWebAPI.Data.Models;
using WoodWebAPI.Data.Models.Customer;
using WoodWebAPI.Data.Models.Order;
using WoodWebAPI.Data.Models.OrderPosition;

namespace WoodWebAPI.Worker.Controller.Commands;

public class CommonChecks(IWorkerCreds workerCreds)
{
    private readonly IWorkerCreds _workerCreds = workerCreds;
    public async Task<OrderModel[]?> CheckOrdersOfCustomer(long chatid, CancellationToken cancellationToken)
    {
        using (HttpClient httpClient = new HttpClient())
        {
            var content = JsonContent.Create(
                new GetOrdersDTO()
                {
                    CustomerTelegramId = chatid,
                });

            var responce = await httpClient.PostAsync($"{_workerCreds.BaseURL}/api/Order/GetOrdersOfCustomer", content, cancellationToken);
            var responseJsonContent = await responce.Content.ReadAsStringAsync(cancellationToken);
            return JsonConvert.DeserializeObject<OrderModel[]?>(responseJsonContent);
        }
    }

    public async Task<GetAdminDTO[]?> GetAdmin()
    {
        using (HttpClient httpClient = new HttpClient())
        {
            var request = await httpClient.GetAsync($"{_workerCreds.BaseURL}/api/Customer/GetAdminList");
            var responce = await request.Content.ReadAsStringAsync();
            if(request.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<GetAdminDTO[]?>(responce);
            }

            return new[] { new GetAdminDTO() };
        };
    }

    public async Task<double> GetVolume(long chatid, int orderid)
    {
        using (HttpClient httpClient = new HttpClient()) 
        {
            var content = JsonContent.Create(new GetOrderPositionsByOrderIdDTO()
            {
                TelegramId = chatid,
                OrderId = orderid
            });
            var request = await httpClient.PostAsync($"{_workerCreds.BaseURL}/api/OrderPosition/GetTotalVolumeOfOrder",content);
            var responseVolume = await request.Content.ReadAsStringAsync();
            var volume = JsonConvert.DeserializeObject<double>(responseVolume);
            return volume;
        }
    }
    public async Task<bool> CheckCustomer(long chatid, CancellationToken cancellationToken)
    {
        if (!cancellationToken.IsCancellationRequested)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                var responce = await httpClient.PostAsync($"{_workerCreds.BaseURL}/api/Customer/GetCustomers", new StringContent(""), cancellationToken);
                var responseJsonContent = await responce.Content.ReadAsStringAsync(cancellationToken);
                GetCustomerModel[] customers = JsonConvert.DeserializeObject<GetCustomerModel[]>(responseJsonContent);

                if (customers != null)
                {
                    foreach (var customer in customers)
                    {
                        try
                        {
                            if (customer.TelegramId == chatid)
                            {
                                return true;
                            }
                        }
                        catch (FormatException ex)
                        {
                            TelegramWorker.Logger
                                 .LogWarning("Main command\n" +
                                "\tНевозможно распарсить идентификатор, скорее всего он не равен типу long", cancellationToken);
                        }

                    }
                }

                return false;
            }
        }

        return false;
    }
}
