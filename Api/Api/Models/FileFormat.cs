using System.Text.Json.Serialization;

namespace Api.Models
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum FileFormat
    {
        Raw,
        GraphML,
        Graph6
    }
}
