namespace API.Entities
{
    public class FileEntity
    {
        public int Id { get; set; } 

        public string FileName { get; set; } 

        public string FilePath { get; set; } 

        public string Checksum { get; set; }

        public long SizeInBytes { get; set; } 

        public string ContentType { get; set; } 

        public DateTime UploadedAt { get; set; } 

        public string UploadedBy { get; set; }
    }
}
