namespace SmartKasir.Client.Services;

public interface IAuthService
{
    bool IsAuthenticated { get; }
    UserInfo? CurrentUser { get; }
    event Action? OnAuthStateChanged;
    
    Task InitializeAsync();
    Task<AuthResult> LoginAsync(string username, string password);
    Task LogoutAsync();
}

public class UserInfo
{
    public int Id { get; set; }
    public string Username { get; set; } = "";
    public string Role { get; set; } = "";
}

public class AuthResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public UserInfo? User { get; set; }
}
