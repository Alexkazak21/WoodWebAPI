using System.Globalization;

namespace WoodWebAPI.Worker;

public class TelegramWorkerCreds : IWorkerCreds
{
    private readonly string _telegtamToken;
    private readonly string _ngrokURL;
    private readonly string _baseUrl;
    private readonly string _mainAdmin;
    private readonly string? _telegramId;
    private readonly decimal _priceForM3;
    private readonly decimal _minPrice;
    private readonly string _paymentToken;

    public string TelegramToken { get => _telegtamToken; }
    public string NgrokURL { get => _ngrokURL; }
    public string BaseURL { get => _baseUrl; }
    public string MainAdmin { get => _mainAdmin; }
    public string? TelegramId { get => _telegramId; }

    public decimal PriceForM3 { get => _priceForM3; }

    public string PaymentToken { get => _paymentToken; }

    public decimal MinPrice { get => _minPrice; }

    public TelegramWorkerCreds(string telegramToken, string ngrokURL, string baseURL, string mainAdmin, string? telegramId, string price, string paymentToken, string minPrice)
    {
        _telegramId = telegramId;
        _baseUrl = baseURL;
        _mainAdmin = mainAdmin;
        _telegtamToken = telegramToken;
        _ngrokURL = ngrokURL;
        _ = decimal.TryParse(price, out _priceForM3);
        _paymentToken = paymentToken;
        _minPrice = Convert.ToDecimal(minPrice, CultureInfo.InvariantCulture);
    }
}