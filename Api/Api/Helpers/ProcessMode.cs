using System.ComponentModel;
using System.Text.Json.Serialization;

namespace Api.Models
{
    /// <summary>
    /// Mode, in which an image file of the graph entity is processed by the script.
    /// <br></br>
    /// <para>AUTO- Sript tries to determine background by itself.</para>
    /// <para>GRID_BG - Hand drawn on grid/lined piece of paper (grid/lined notebook etc.)</para>
    /// <para>CLEAN_BG - Hand drawn on empty uniform color background (on board, empty piece of paper, editor (paint)</para>
    /// <para>PRINTED - Printed (e.g. from paper, publication, book...)</para>
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ProcessMode
    {
        AUTO = 0,
        GRID_BG = 1,
        CLEAN_BG = 2,
        PRINTED = 3
    }
}
