using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ophelia.Integration.CDN
{
    public class FileUploadResult
    {
        public bool InProcess { get; set; }
        public bool Failed { get; set; }
        public string UploadedFile { get; set; }
        public string Description { get; set; }
        public long UploadedFileLength { get; set; }
    }
}
