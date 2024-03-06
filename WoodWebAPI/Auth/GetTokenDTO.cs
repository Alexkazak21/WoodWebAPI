namespace WoodWebAPI.Auth;

public class GetTokenDTO
{
    public string Token { get; set; } = string.Empty;
    public DateTime Expiration { get; set; }
}
