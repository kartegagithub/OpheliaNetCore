using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ophelia.Data
{
    public class DataConfiguration : IDisposable
    {
        public List<string> NamespacesToIgnore { get; set; }
        public bool UseNamespaceAsSchema { get; set; }
        public bool PrimaryKeyContainsEntityName { get; set; }
        public bool AllowStructureAutoCreation { get; set; }
        public bool AllowLinkedDatabases { get; set; }
        public int DefaultStringColumnSize { get; set; }
        public int DefaultDecimalColumnPrecision { get; set; }
        public int DefaultDecimalColumnScale { get; set; }
        public bool EnableLazyLoading { get; set; }
        public bool LogSQL { get; set; }
        public bool LogEntityLoads { get; set; }
        public string OracleStringColumnCollation { get; set; }
        public string DatabaseVersion { get; set; }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
        public DataConfiguration()
        {
            this.NamespacesToIgnore = new List<string>();
            this.UseNamespaceAsSchema = true;
            this.PrimaryKeyContainsEntityName = false;
            this.AllowStructureAutoCreation = true;
            this.DefaultStringColumnSize = 255;
            this.DefaultDecimalColumnScale = 5;
            this.DefaultDecimalColumnPrecision = 38;
            this.EnableLazyLoading = false;
        }
    }
}
