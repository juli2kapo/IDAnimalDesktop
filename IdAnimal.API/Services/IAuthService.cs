using IdAnimal.Shared.DTOs;

namespace IdAnimal.API.Services;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request);
    Task<LoginResponse?> RegisterAsync(RegisterRequest request);
    Task<int?> GetUserIdFromTokenAsync(string token);
}
