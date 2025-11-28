using System.IO;
using SmartKasir.Application.DTOs;
using ClientUserDto = SmartKasir.Client.Services.UserDto;
using AppUserDto = SmartKasir.Application.DTOs.UserDto;

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

    public async Task<AuthResult> LoginAsync(string username, string password)
    {
        try
        {
            var request = new LoginRequest(username, password);
            var response = await _api.LoginAsync(request);

            _currentUser = ToClientUserDto(response.User);
            _currentToken = response.Token;
            _refreshToken = response.RefreshToken;

            SaveCredentials();
            OnAuthStatusChanged(new AuthStatusChangedEventArgs
            {
                IsAuthenticated = true,
                User = _currentUser
            });

            return new AuthResult(true, response.Token, response.RefreshToken, _currentUser, null);
        }
        catch (Exception ex)
        {
            return new AuthResult(false, null, null, null, ex.Message);
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
            _currentUser = ToClientUserDto(response.User);

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
        // Save to secure storage (Windows Credential Manager)
        try
        {
            var credentialSet = new System.Net.NetworkCredential
            {
                UserName = _currentUser?.Username ?? string.Empty,
                Password = _currentToken ?? string.Empty
            };

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

    private static ClientUserDto? ToClientUserDto(AppUserDto? appUser)
    {
        if (appUser == null) return null;
        return new ClientUserDto(appUser.Id, appUser.Username, appUser.Role, appUser.IsActive);
    }
}
