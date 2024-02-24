namespace WoodWebAPI.Data.Models;

public class GetAdminDTO
{
    public int Id { get; set; }
    public string? TelegramUsername { get; set; }
    public string? TelegramId { get; set; }
    public int AdminRole { get; set; } // 1 - IsAdmin  0 - User
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
