using System.Text.Json.Serialization;
namespace SharedApiUtils.RabbitMq;

public class RabbitMqConfiguration
{
    [JsonPropertyName("Host")]
    public string Host { get; set; }

    [JsonPropertyName("Username")]
    public string Username { get; set; }

    [JsonPropertyName("Password")]
    public string Password { get; set; }
}
