namespace Pixlmint.Aioniq.Model;

public class User
{
    public int Id { get; set; }
    public string GoogleUserId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Picture { get; set; } = string.Empty;
    public UserTokens? Tokens { get; set; }
    public int UserTokensId { get; set; }

    public List<UserRefreshToken> RefreshTokens { get; set; } = [];
}

public class UserTokens
{
    public int Id { get; set; }
    public string AccessToken { get; set; } = string.Empty;
    public string? RefreshToken { get; set; }
    public string? TokenType { get; set; }
    public long? ExpiresInSeconds { get; set; }
    public User? User { get; set; }
    public int UserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
}

public class UserRefreshToken
{
    public int Id { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public User? User { get; set; }
}
