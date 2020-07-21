using System.ComponentModel;
using System.Text.Json.Serialization;

namespace Api.Models
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ProcessMode
    {
        GRID_BG = 1,
        CLEAN_BG = 2,
        PRINTED = 3
    }
}
