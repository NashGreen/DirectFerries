using System.Text.Json.Serialization;

namespace DirectFerries.Models
{
    public class User
    {
        [JsonPropertyName("id")] 
        public int Id { get; set; }

        [JsonPropertyName("username")] 
        public string Username { get; set; } = string.Empty;

        [JsonPropertyName("email")] 
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("firstName")] 
        public string FirstName { get; set; } = string.Empty;

        [JsonPropertyName("lastName")] 
        public string LastName { get; set; } = string.Empty;

        [JsonPropertyName("gender")] 
        public string Gender { get; set; } = string.Empty;

        [JsonPropertyName("image")] 
        public string Image { get; set; } = string.Empty;

        [JsonPropertyName("accessToken")] 
        public string Token { get; set; } = string.Empty;
    }

    public class LoginRequest
    {
        [JsonPropertyName("username")] 
        public string Username { get; set; } = string.Empty;

        [JsonPropertyName("password")] 
        public string Password { get; set; } = string.Empty;
    }

    public class LoginResponse
    {
        [JsonPropertyName("id")] 
        public int Id { get; set; }

        [JsonPropertyName("username")] 
        public string Username { get; set; } = string.Empty;

        [JsonPropertyName("email")] 
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("firstName")] 
        public string FirstName { get; set; } = string.Empty;

        [JsonPropertyName("lastName")] 
        public string LastName { get; set; } = string.Empty;

        [JsonPropertyName("gender")] 
        public string Gender { get; set; } = string.Empty;

        [JsonPropertyName("image")] 
        public string Image { get; set; } = string.Empty;

        [JsonPropertyName("token")] 
        public string Token { get; set; } = string.Empty;
    }
}
