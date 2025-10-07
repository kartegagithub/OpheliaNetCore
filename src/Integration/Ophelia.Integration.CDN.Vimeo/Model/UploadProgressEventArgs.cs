namespace Ophelia.Integration.CDN.Vimeo.Model
{
    public class UploadProgressEventArgs
    {
        public long Total { get; set; }
        public long Uploaded { get; set; }
        public long Remaining { get; set; }
    }
}
