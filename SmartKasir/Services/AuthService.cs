using System.Net.Http.Json;
using Blazored.LocalStorage;

namespace SmartKasir.Client.Services;

public class AuthService : IAuthService
{
    private readonly HttpClient _http;
    private readonly ILocalStorageService _localStorage;
    private const string AuthKey = "smartkasir_auth";

    public bool IsAuthenticated => CurrentUser != null;
    public UserInfo? CurrentUser { get; private set; }
    public event Action? OnAuthStateChanged;

    public AuthService(HttpClient http, ILocalStorageService localStorage)
    {
        _http = http;
        _localStorage = localStorage;
    }

    public async Task InitializeAsync()
    {
        try
        {
            CurrentUser = await _localStorage.GetItemAsync<UserInfo>(AuthKey);
            OnAuthStateChanged?.Invoke();
        }
        catch
        {
            CurrentUser = null;
        }
    }

    public async Task<AuthResult> LoginAsync(string username, string password)
    {
        try
        {
            // Try API login first
            var response = await _http.PostAsJsonAsync("/api/auth/login", new { username, password });
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<AuthResult>();
                if (result?.Success == true && result.User != null)
                {
                    CurrentUser = result.User;
                    await _localStorage.SetItemAsync(AuthKey, CurrentUser);
                    OnAuthStateChanged?.Invoke();
                    return result;
                }
            }
        }
        catch
        {
            // Fallback to demo credentials if API unavailable
        }

        // Demo credentials fallback
        if ((username == "admin" && password == "admin") ||
            (username == "kasir" && password == "kasir"))
        {
            CurrentUser = new UserInfo
            {
                Id = username == "admin" ? 1 : 2,
                Username = username,
                Role = username == "admin" ? "Admin" : "Kasir"
            };
            await _localStorage.SetItemAsync(AuthKey, CurrentUser);
            OnAuthStateChanged?.Invoke();
            return new AuthResult { Success = true, User = CurrentUser };
        }

        return new AuthResult { Success = false, ErrorMessage = "Username atau password salah" };
    }

    public async Task LogoutAsync()
    {
        CurrentUser = null;
        await _localStorage.RemoveItemAsync(AuthKey);
        OnAuthStateChanged?.Invoke();
    }
}
