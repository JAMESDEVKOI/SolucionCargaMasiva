namespace FileControl.Infrastructure.Storage
{
    public class SeaweedFSSettings
    {
        public const string SectionName = "SeaweedFS";
        public string MasterUrl { get; set; } = string.Empty;
        public string FilerUrl { get; set; } = string.Empty;
    }
}
