using System.Text.Json.Serialization;

namespace Api.Models
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum GraphFormat
    {
        RawImage,
        GraphML,
        Graph6
    }
}
