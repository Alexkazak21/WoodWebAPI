namespace WoodWebAPI.Worker;

public interface IWorkerCreds
{
    public string TelegramToken {  get; }
    public string NgrokURL { get; }
    public string BaseURL { get; }
    public string MainAdmin { get; }
    public string? TelegramId { get; }
    public decimal PriceForM3 { get; }
    public string PaymentToken { get; }
    public decimal MinPrice { get; }
}
