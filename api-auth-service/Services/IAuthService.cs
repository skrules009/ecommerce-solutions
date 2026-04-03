using api_auth_service.DTOs;
using api_auth_service.Models;

namespace api_auth_service.Services
{
    public interface IAuthService
    {
        Task<TokenResponseDto> RegisterAsync(RegisterDto request);
        Task<TokenResponseDto> LoginAsync(LoginDto request);
        Task<TokenResponseDto> RefreshTokenAsync(RefreshTokenDto request);
        Task<ValidateTokenResponseDto> ValidateTokenAsync(string token);
        Task<bool> RevokeTokenAsync(string refreshToken);
        Task<bool> LogoutAsync(int userId);
    }
}
