using System.Text.Json.Serialization;

namespace Api.Models
{
    /// <summary>
    /// Graph file formats of the actual files in the storage.
    /// <para>Each GraphEntity contains one file for each type below.</para>
    /// <para>RawImage is the default image extension of the image file.</para>
    /// <para>To produce other files, an image file needs to be processed.</para>
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum GraphFormat
    {
        RawImage,
        GraphML,
        Graph6
    }
}
