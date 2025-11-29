using System.IO;
using SmartKasir.Application.DTOs;

namespace SmartKasir.Client.Services;

/// <summary>
/// Implementation dari IAuthService
/// </summary>
public class AuthService : IAuthService
{
    private readonly ISmartKasirApi _api;
    private ClientUserDto? _currentUser;
    private string? _currentToken;
    private string? _refreshToken;

    public event EventHandler<AuthStatusChangedEventArgs>? AuthStatusChanged;

    public bool IsAuthenticated => _currentUser != null && !string.IsNullOrEmpty(_currentToken);

    public ClientUserDto? CurrentUser => _currentUser;

    public string? CurrentToken => _currentToken;

    public AuthService(ISmartKasirApi api)
    {
        _api = api;
        LoadStoredCredentials();
    }

    public async Task<ClientAuthResult> LoginAsync(string username, string password)
    {
        try
        {
            Console.WriteLine($"[AuthService] LoginAsync called with username: {username}");
            
            // TEST: Allow test login without server
            // Admin login: admin/admin
            if (username == "admin" && password == "admin")
            {
                Console.WriteLine($"[AuthService] Test Admin login detected");
                _currentUser = new ClientUserDto(Guid.NewGuid(), "admin", SmartKasir.Core.Enums.UserRole.Admin, true);
                _currentToken = "test-token-" + Guid.NewGuid().ToString();
                _refreshToken = "test-refresh-" + Guid.NewGuid().ToString();

                SaveCredentials();
                OnAuthStatusChanged(new AuthStatusChangedEventArgs
                {
                    IsAuthenticated = true,
                    User = _currentUser
                });

                Console.WriteLine($"[AuthService] Admin login successful");
                return new ClientAuthResult(true, _currentToken, _refreshToken, _currentUser, null);
            }
            
            // Cashier login: kasir/kasir
            if (username == "kasir" && password == "kasir")
            {
                Console.WriteLine($"[AuthService] Test Cashier login detected");
                _currentUser = new ClientUserDto(Guid.NewGuid(), "kasir", SmartKasir.Core.Enums.UserRole.Cashier, true);
                _currentToken = "test-token-" + Guid.NewGuid().ToString();
                _refreshToken = "test-refresh-" + Guid.NewGuid().ToString();

                SaveCredentials();
                OnAuthStatusChanged(new AuthStatusChangedEventArgs
                {
                    IsAuthenticated = true,
                    User = _currentUser
                });

                Console.WriteLine($"[AuthService] Cashier login successful");
                return new ClientAuthResult(true, _currentToken, _refreshToken, _currentUser, null);
            }

            Console.WriteLine($"[AuthService] Attempting server login");
            var request = new LoginRequest(username, password);
            var response = await _api.LoginAsync(request);

            _currentUser = ClientUserDto.FromUserDto(response.User);
            _currentToken = response.Token;
            _refreshToken = response.RefreshToken;

            SaveCredentials();
            OnAuthStatusChanged(new AuthStatusChangedEventArgs
            {
                IsAuthenticated = true,
                User = _currentUser
            });

            Console.WriteLine($"[AuthService] Server login successful");
            return new ClientAuthResult(true, response.Token, response.RefreshToken, _currentUser, null);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AuthService] LoginAsync error: {ex.Message}");
            Console.WriteLine($"[AuthService] Stack trace: {ex.StackTrace}");
            return new ClientAuthResult(false, null, null, null, ex.Message);
        }
    }

    public async Task LogoutAsync()
    {
        try
        {
            await _api.LogoutAsync();
        }
        catch
        {
            // Ignore errors during logout
        }
        finally
        {
            _currentUser = null;
            _currentToken = null;
            _refreshToken = null;
            ClearStoredCredentials();

            OnAuthStatusChanged(new AuthStatusChangedEventArgs
            {
                IsAuthenticated = false,
                User = null
            });
        }
    }

    public async Task<bool> RefreshTokenAsync()
    {
        if (string.IsNullOrEmpty(_refreshToken))
        {
            return false;
        }

        try
        {
            var request = new RefreshTokenRequest(_refreshToken);
            var response = await _api.RefreshTokenAsync(request);

            _currentToken = response.Token;
            _refreshToken = response.RefreshToken;
            _currentUser = ClientUserDto.FromUserDto(response.User);

            SaveCredentials();
            return true;
        }
        catch
        {
            await LogoutAsync();
            return false;
        }
    }

    private void SaveCredentials()
    {
        try
        {
            // In production, use Windows Credential Manager or similar
            // For now, we'll use isolated storage
            var appData = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "SmartKasir");

            if (!Directory.Exists(appData))
            {
                Directory.CreateDirectory(appData);
            }

            var credFile = Path.Combine(appData, ".credentials");
            File.WriteAllText(credFile, $"{_currentUser?.Id}|{_currentToken}|{_refreshToken}");
        }
        catch
        {
            // Ignore credential save errors
        }
    }

    private void LoadStoredCredentials()
    {
        try
        {
            var appData = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "SmartKasir");

            var credFile = Path.Combine(appData, ".credentials");
            if (File.Exists(credFile))
            {
                var content = File.ReadAllText(credFile);
                var parts = content.Split('|');
                if (parts.Length == 3)
                {
                    _currentToken = parts[1];
                    _refreshToken = parts[2];
                    // Note: We don't restore _currentUser here, it will be fetched on demand
                }
            }
        }
        catch
        {
            // Ignore credential load errors
        }
    }

    private void ClearStoredCredentials()
    {
        try
        {
            var appData = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "SmartKasir");

            var credFile = Path.Combine(appData, ".credentials");
            if (File.Exists(credFile))
            {
                File.Delete(credFile);
            }
        }
        catch
        {
            // Ignore errors
        }
    }

    private void OnAuthStatusChanged(AuthStatusChangedEventArgs args)
    {
        AuthStatusChanged?.Invoke(this, args);
    }
}
