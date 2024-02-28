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

    public TelegramWorkerCreds()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Configuration.Sources.Clear();
        IConfigurationRoot configuration = builder
                .Configuration
                .AddJsonFile("appsettings.json", false)
                .AddJsonFile("appsettings.Development.json", false)
                .AddJsonFile("appsettings.local.json", true)
                .AddJsonFile("Properties\\launchSettings.json")
                .Build();

        _telegramId = configuration.GetSection("admin").GetValue<string>("TelegramId") ?? throw new ArgumentException("TelegramId", "TelegramId must be declared");
        _baseUrl = configuration.GetSection("profiles").GetSection("http").GetValue<string>("applicationUrl") ?? throw new ArgumentNullException("BaseUrl", "BaseUrl field must be specified");
        _mainAdmin = configuration.GetSection("admin").GetValue<string>("Username") ?? throw new ArgumentNullException("Username", "Username must be declared");
        _telegtamToken = configuration.GetValue<string>("TelegramToken") ?? throw new ArgumentNullException("TelegramToken", "Telegtam Token field must be specified");
        _ngrokURL = configuration.GetSection("ngrok").GetValue<string>("URL") ?? throw new ArgumentNullException("NGROK URL", " NGROK URL must be specified");
        _ = decimal.TryParse(configuration.GetValue<string>("price") ?? throw new ArgumentException("Price", "Price must be defined"), out _priceForM3);
        _paymentToken = configuration.GetValue<string>("paymentToken") ?? throw new ArgumentException("paymentToken", "paymentToken must be defined");
        _minPrice = Convert.ToDecimal(configuration.GetValue<string>("minPrice") ?? throw new ArgumentException("minPrice", "minPrice must be defined"), CultureInfo.InvariantCulture);
    }
}