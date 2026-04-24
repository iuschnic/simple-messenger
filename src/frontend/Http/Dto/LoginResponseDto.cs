namespace Http.Dto;

using System.Text.Json.Serialization;

public class LoginResponseDto
{
    [JsonPropertyName("token")]
    public string Token { get; set; }
}