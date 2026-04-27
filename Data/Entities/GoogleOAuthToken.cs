namespace Work_Dashboard.Data.Entities;

public class GoogleOAuthToken
{
    public int TokenId { get; set; }
    public string? ClientId { get; set; }
    public string? ClientSecret { get; set; }
    public string? RefreshToken { get; set; }
    public string? AccessToken { get; set; }
    public DateTime? TokenExpiry { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
