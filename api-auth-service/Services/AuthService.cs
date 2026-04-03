using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using BCrypt.Net;
using api_auth_service.Data;
using api_auth_service.DTOs;
using api_auth_service.Models;
using api_auth_service.Repositories;
using Microsoft.EntityFrameworkCore;

namespace api_auth_service.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPermissionRepository _permissionRepository;
        private readonly AuthDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            IUserRepository userRepository,
            IPermissionRepository permissionRepository,
            AuthDbContext context,
            IConfiguration configuration,
            ILogger<AuthService> logger)
        {
            _userRepository = userRepository;
            _permissionRepository = permissionRepository;
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<TokenResponseDto> RegisterAsync(RegisterDto request)
        {
            // Check if user already exists
            if (await _userRepository.ExistsAsync(request.Email, request.Username))
            {
                throw new InvalidOperationException("Email or username already exists");
            }

            // Get default User role
            var userRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "User");
            if (userRole == null)
            {
                throw new InvalidOperationException("Default user role not found");
            }

            // Create new user
            var user = new User
            {
                Email = request.Email,
                Username = request.Username,
                FirstName = request.FirstName,
                LastName = request.LastName,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                RoleId = userRole.Id,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _userRepository.CreateAsync(user);
            _logger.LogInformation("New user registered: {Username}", user.Username);

            // Generate tokens
            var userFull = await _userRepository.GetWithRoleAndPermissionsAsync(user.Id);
            return await GenerateTokenResponseAsync(userFull!);
        }

        public async Task<TokenResponseDto> LoginAsync(LoginDto request)
        {
            // Find user by username or email
            var user = await _userRepository.GetByUsernameAsync(request.Username)
                ?? await _userRepository.GetByEmailAsync(request.Username);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                throw new UnauthorizedAccessException("Invalid username or password");
            }

            if (!user.IsActive)
            {
                throw new UnauthorizedAccessException("User account is inactive");
            }

            // Update last login time
            user.LastLoginAt = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);
            _logger.LogInformation("User logged in: {Username}", user.Username);

            // Generate tokens
            var userFull = await _userRepository.GetWithRoleAndPermissionsAsync(user.Id);
            return await GenerateTokenResponseAsync(userFull!);
        }

        public async Task<TokenResponseDto> RefreshTokenAsync(RefreshTokenDto request)
        {
            var refreshToken = await _context.RefreshTokens
                .Include(rt => rt.User)
                .ThenInclude(u => u.Role)
                .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken && !rt.IsRevoked);

            if (refreshToken == null || refreshToken.ExpiresAt < DateTime.UtcNow)
            {
                throw new UnauthorizedAccessException("Invalid or expired refresh token");
            }

            var user = await _userRepository.GetWithRoleAndPermissionsAsync(refreshToken.User.Id);
            if (user == null || !user.IsActive)
            {
                throw new UnauthorizedAccessException("User not found or inactive");
            }

            _logger.LogInformation("Token refreshed for user: {Username}", user.Username);
            return await GenerateTokenResponseAsync(user);
        }

        public async Task<ValidateTokenResponseDto> ValidateTokenAsync(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Secret"] ?? throw new InvalidOperationException("JWT secret not configured"));

                var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = _configuration["Jwt:Audience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
                {
                    return new ValidateTokenResponseDto { IsValid = false, Message = "Invalid token claims" };
                }

                var user = await _userRepository.GetWithRoleAndPermissionsAsync(userId);
                if (user == null || !user.IsActive)
                {
                    return new ValidateTokenResponseDto { IsValid = false, Message = "User not found or inactive" };
                }

                var userDto = MapToUserDto(user);
                return new ValidateTokenResponseDto
                {
                    IsValid = true,
                    Message = "Token is valid",
                    User = userDto
                };
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Token validation failed: {Message}", ex.Message);
                return new ValidateTokenResponseDto { IsValid = false, Message = "Token validation failed" };
            }
        }

        public async Task<bool> RevokeTokenAsync(string refreshToken)
        {
            var token = await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == refreshToken);
            if (token == null)
                return false;

            token.IsRevoked = true;
            await _context.SaveChangesAsync();
            _logger.LogInformation("Refresh token revoked");
            return true;
        }

        public async Task<bool> LogoutAsync(int userId)
        {
            var tokens = await _context.RefreshTokens
                .Where(rt => rt.UserId == userId && !rt.IsRevoked)
                .ToListAsync();

            foreach (var token in tokens)
            {
                token.IsRevoked = true;
            }

            if (tokens.Any())
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("User {UserId} logged out, {TokenCount} tokens revoked", userId, tokens.Count);
            }

            return true;
        }

        private async Task<TokenResponseDto> GenerateTokenResponseAsync(User user)
        {
            var accessToken = GenerateAccessToken(user);
            var refreshToken = GenerateRefreshToken();

            // Save refresh token to database
            var refreshTokenEntity = new RefreshToken
            {
                UserId = user.Id,
                Token = refreshToken,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(int.Parse(_configuration["Jwt:RefreshExpiryDays"] ?? "7")),
                IsRevoked = false
            };

            _context.RefreshTokens.Add(refreshTokenEntity);
            await _context.SaveChangesAsync();

            var expiryMinutes = int.Parse(_configuration["Jwt:ExpiryMinutes"] ?? "15");

            return new TokenResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresIn = expiryMinutes * 60,
                TokenType = "Bearer",
                User = MapToUserDto(user)
            };
        }

        private string GenerateAccessToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Secret"] ?? throw new InvalidOperationException("JWT secret not configured"));

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("FirstName", user.FirstName),
                new Claim("LastName", user.LastName),
                new Claim(ClaimTypes.Role, user.Role?.Name ?? "User")
            };

            // Add permissions as claims
            if (user.Role?.RolePermissions != null)
            {
                foreach (var perm in user.Role.RolePermissions)
                {
                    claims.Add(new Claim("permission", perm.Permission?.Name ?? ""));
                }
            }

            var expiryMinutes = int.Parse(_configuration["Jwt:ExpiryMinutes"] ?? "15");

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(expiryMinutes),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }

        private UserDto MapToUserDto(User user)
        {
            return new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                Username = user.Username,
                FirstName = user.FirstName,
                LastName = user.LastName,
                IsActive = user.IsActive,
                RoleName = user.Role?.Name ?? "User",
                Permissions = user.Role?.RolePermissions?
                    .Select(rp => rp.Permission?.Name ?? "")
                    .Where(p => !string.IsNullOrEmpty(p))
                    .ToList() ?? new List<string>()
            };
        }
    }
}
