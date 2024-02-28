namespace WoodWebAPI.Data.Entities;

public class IsAdmin
{
    public int Id { get; set; }
    public string? TelegramUsername { get; set; }
    public string? TelegramId { get; set; }
    public int AdminRole {  get; set; } // 1 - IsAdmin  0 - User
    public DateTime CreatedAt { get; set; }
}
