namespace Api.Helpers
{
    public class AppSettings
    {
        public Secrets Secrets { get; set; }
        public StoragePaths StoragePaths { get; set; }
    }

    public class Secrets
    {
        public string Secret { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string StorageFile { get; set; }
        public string RedirectUri { get; set; }
    }

    public class StoragePaths
    {
        public string GoogleStorageDirectory { get; set; }
        public string ScriptFullPath { get; set; }
        public string UploadsDirectory { get; set; }
    }
}
