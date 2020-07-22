using System.ComponentModel;
using System.Text.Json.Serialization;

namespace Api.Models
{
    /// <summary>
    /// <para>GRID_BG - Hand drawn on grid/lined piece of paper (grid/lined notebook etc.)</para>
    /// <para>CLEAN_BG - Hand drawn on empty uniform color background (on board, empty piece of paper, editor (paint)</para>
    /// <para>PRINTED - Printed (e.g. from paper, publication, book...)</para>
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ProcessMode
    {
        GRID_BG = 1,
        CLEAN_BG = 2,
        PRINTED = 3
    }
}
