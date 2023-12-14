using System;
using System.Collections.Generic;

namespace Ophelia.Data
{
    public class DataConfiguration : EntityFramework.DataConfiguration
    {
        public List<string> NamespacesToIgnore { get; set; }
        public bool UseDBLevelPaging { get; set; }
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
        public string OracleStringColumnCollation { get; set; } = "";
        public Dictionary<Type, string> LinkedDatabases { get; set; }
        public string DatabaseVersion { get; set; } = "";
        public int ObjectNameCharLimit { get; set; }
        public bool UseUppercaseObjectNames { get; set; }
        public bool QueryBooleanAsBinary { get; set; }
        public Func<string, string> StringParameterFormatter { get; set; }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
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
            this.LinkedDatabases = new Dictionary<Type, string>();
            this.QueryBooleanAsBinary = true;
            this.UseDBLevelPaging = true;
        }
    }
}
