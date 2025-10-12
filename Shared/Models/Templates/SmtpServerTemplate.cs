using System.Text.Json.Serialization;

namespace DietiEstate.Shared.Models.Templates;

public class SmtpServerTemplate
{
    [JsonPropertyName("SERVER")]
    public string Server { get; set; } = string.Empty;
    
    [JsonPropertyName("PORT")]
    public int Port { get; set; }
    
    [JsonPropertyName("USERNAME")]
    public string Username { get; set; } = string.Empty;
    
    [JsonPropertyName("PASSWORD")]
    public string Password { get; set; } = string.Empty;
    
    [JsonPropertyName("FROMEMAIL")]
    public string FromEmail { get; set; } = string.Empty;
    
    [JsonPropertyName("FROMNAME")]
    public string FromName { get; set; } = string.Empty;

}