using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ophelia.Service
{
    public class WebApiObjectRequest<T> : IDisposable
    {
        public long ID { get; set; }
        public long LanguageID { get; set; }
        public string Name { get; set; }
        public T Data { get; set; }
        public Dictionary<string, object> Parameters { get; set; }
        public List<FileData> Files { get; set; }
        public WebApiObjectRequest()
        {
            this.Parameters = new Dictionary<string, object>();
            this.Files = new List<FileData>();
        }

        public void Dispose()
        {
            this.Parameters = null;
            this.Files = null;
        }
    }

    public class FileData
    {
        public string KeyName { get; set; }
        public string FileName { get; set; }
        public byte[] ByteData { get; set; }
        public string Base64Data { get; set; }
    }
}
