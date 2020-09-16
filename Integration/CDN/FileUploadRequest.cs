using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ophelia.Integration.CDN
{
    public class FileUploadRequest
    {
        public long LocalID { get; set; }
        public string Path { get; set; }
        public UploadTicket Ticket { get; set; }
        public long ChunkSize { get; set; }
        public long FromRange { get; set; }
        public long ReplaceVideoId { get; set; }
        public FileUploadRequest()
        {
            this.ChunkSize = 131072;
        }
    }
}
