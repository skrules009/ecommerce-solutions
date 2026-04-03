namespace api_auth_service.DTOs
{
    public class ValidateTokenResponseDto
    {
        public bool IsValid { get; set; }
        public string Message { get; set; } = string.Empty;
        public UserDto? User { get; set; }
    }
}
