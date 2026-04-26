using System.Text.Json.Serialization;

namespace Http.Dto;


public class ContactDto
{
    [JsonPropertyName("contactUser")]
    public UserDto ContactUser { get; set; }
    [JsonPropertyName("contactName")]
    public string ContactName { get; set; } = string.Empty;
}