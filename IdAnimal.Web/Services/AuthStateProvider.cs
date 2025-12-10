using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace IdAnimal.Web.Services;

public class AuthStateProvider : AuthenticationStateProvider
{
    private readonly ProtectedLocalStorage _localStorage;
    private const string TokenKey = "authToken";
    private const string UserIdKey = "userId";
    private const string EmailKey = "email";
    private const string FullNameKey = "fullName";

    public AuthStateProvider(ProtectedLocalStorage localStorage)
    {
        _localStorage = localStorage;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var token = await GetTokenAsync();
            var userId = await GetUserIdAsync();
            var email = await GetEmailAsync();
            var fullName = await GetFullNameAsync();

            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(userId))
            {
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Email, email ?? ""),
                new Claim(ClaimTypes.Name, fullName ?? "")
            };

            var identity = new ClaimsIdentity(claims, "jwt");
            var user = new ClaimsPrincipal(identity);

            return new AuthenticationState(user);
        }
        catch
        {
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }
    }

    public async Task LoginAsync(string token, int userId, string email, string fullName)
    {
        await _localStorage.SetAsync(TokenKey, token);
        await _localStorage.SetAsync(UserIdKey, userId.ToString());
        await _localStorage.SetAsync(EmailKey, email);
        await _localStorage.SetAsync(FullNameKey, fullName);

        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public async Task LogoutAsync()
    {
        await _localStorage.DeleteAsync(TokenKey);
        await _localStorage.DeleteAsync(UserIdKey);
        await _localStorage.DeleteAsync(EmailKey);
        await _localStorage.DeleteAsync(FullNameKey);

        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public async Task<string?> GetTokenAsync()
    {
        try
        {
            var result = await _localStorage.GetAsync<string>(TokenKey);
            return result.Success ? result.Value : null;
        }
        catch
        {
            return null;
        }
    }

    public async Task<string?> GetUserIdAsync()
    {
        try
        {
            var result = await _localStorage.GetAsync<string>(UserIdKey);
            return result.Success ? result.Value : null;
        }
        catch
        {
            return null;
        }
    }

    public async Task<string?> GetEmailAsync()
    {
        try
        {
            var result = await _localStorage.GetAsync<string>(EmailKey);
            return result.Success ? result.Value : null;
        }
        catch
        {
            return null;
        }
    }

    public async Task<string?> GetFullNameAsync()
    {
        try
        {
            var result = await _localStorage.GetAsync<string>(FullNameKey);
            return result.Success ? result.Value : null;
        }
        catch
        {
            return null;
        }
    }
}
