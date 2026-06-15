using System;

namespace MVC5_FileManager.Models
{
    public class FileModel
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string UploadedBy { get; set; }
        public string RoleAllowed { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
