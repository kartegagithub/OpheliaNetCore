using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ophelia.Integration.CDN
{
    public class UploadProgressEventArgs
    {
        public long Total { get; set; }
        public long Uploaded { get; set; }
        public long Remaining { get; set; }
    }
}
