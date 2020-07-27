namespace Api.Models
{
    /// <summary>
    /// Parameters of the image processing script.
    /// </summary>
    public class ProcessRequest
    {
        public ProcessMode Mode { get; set; }
        public string Lorem { get; set; }
        public string Ipsum { get; set; }
    }
}
