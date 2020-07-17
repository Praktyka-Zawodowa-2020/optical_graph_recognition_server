using System.Text.Json.Serialization;

namespace Api.Models
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum GraphFormat
    {
        Raw,
        GraphML,
        Graph6
    }
}
