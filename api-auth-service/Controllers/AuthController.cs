using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using api_auth_service.DTOs;
using api_auth_service.Services;

namespace api_auth_service.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// Register a new user
        /// </summary>
        /// <param name="request">Registration details</param>
        /// <returns>Access token and refresh token</returns>
        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var response = await _authService.RegisterAsync(request);
                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Registration failed: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError("Registration error: {Message}", ex.Message);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// User login
        /// </summary>
        /// <param name="request">Login credentials</param>
        /// <returns>Access token and refresh token</returns>
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var response = await _authService.LoginAsync(request);
                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Login failed: {Message}", ex.Message);
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError("Login error: {Message}", ex.Message);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Refresh access token
        /// </summary>
        /// <param name="request">Refresh token</param>
        /// <returns>New access token and refresh token</returns>
        [HttpPost("refresh")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var response = await _authService.RefreshTokenAsync(request);
                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Token refresh failed: {Message}", ex.Message);
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError("Token refresh error: {Message}", ex.Message);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Validate access token
        /// </summary>
        /// <param name="request">Token to validate</param>
        /// <returns>Validation result with user information</returns>
        [HttpPost("validate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ValidateToken([FromBody] ValidateTokenDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var response = await _authService.ValidateTokenAsync(request.Token);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError("Token validation error: {Message}", ex.Message);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Logout user (revoke all refresh tokens)
        /// </summary>
        /// <returns>Success message</returns>
        [Authorize]
        [HttpPost("logout")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Logout()
        {
            try
            {
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
                {
                    return Unauthorized(new { message = "Invalid token" });
                }

                await _authService.LogoutAsync(userId);
                return Ok(new { message = "Logged out successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError("Logout error: {Message}", ex.Message);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get current user profile (requires authentication)
        /// </summary>
        /// <returns>Current user information</returns>
        [Authorize]
        [HttpGet("me")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult GetCurrentUser()
        {
            try
            {
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                var usernameClaim = User.FindFirst(System.Security.Claims.ClaimTypes.Name);
                var emailClaim = User.FindFirst(System.Security.Claims.ClaimTypes.Email);
                var roleClaim = User.FindFirst(System.Security.Claims.ClaimTypes.Role);

                if (userIdClaim == null)
                {
                    return Unauthorized(new { message = "Invalid token" });
                }

                return Ok(new
                {
                    id = userIdClaim.Value,
                    username = usernameClaim?.Value,
                    email = emailClaim?.Value,
                    role = roleClaim?.Value
                });
            }
            catch (Exception ex)
            {
                _logger.LogError("Get user error: {Message}", ex.Message);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
    }
}
