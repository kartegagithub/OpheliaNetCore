using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExcelDataReader;
using Ophelia.Data.Exporter.Controls;

namespace Ophelia.Data.Importer
{
    public class ExcelImporter : IImporter
    {
        public bool FirstRowIsHeader { get; set; }
        public Dictionary<int, string> ColumnMappings { get; set; }
        public ExcelImporter()
        {
            this.ColumnMappings = new Dictionary<int, string>();
        }
        public List<TResult> Import<TResult>(Stream stream)
        {
            var list = new List<TResult>();
            using (var ds = this.ReadInternal(stream))
            {
                if (ds.Tables.Count == 0)
                    return list;


                var rowIndex = 0;
                foreach (System.Data.DataRow row in ds.Tables[0].Rows)
                {
                    if (this.FirstRowIsHeader && rowIndex == 0)
                    {
                        rowIndex++;
                        continue;
                    }

                    var obj = Activator.CreateInstance(typeof(TResult));
                    var columnIndex = 0;
                    foreach (System.Data.DataColumn column in ds.Tables[0].Columns)
                    {
                        var propertyName = "";
                        if (this.ColumnMappings.Count > 0 && this.ColumnMappings.ContainsKey(columnIndex))
                        {
                            propertyName = this.ColumnMappings[columnIndex];
                        }
                        else
                        {
                            propertyName = column.ColumnName;
                        }
                        obj.SetPropertyValue(propertyName, row[columnIndex]);
                        columnIndex++;
                    }
                    rowIndex++;
                }
            }
            return list;
        }

        public DataSet Import(Stream stream)
        {
            var ds = this.ReadInternal(stream);
            return ds;
        }

        public List<Grid> ImportAsGrid(Stream stream)
        {
            throw new NotImplementedException();
        }
        private System.Data.DataSet ReadInternal(Stream stream)
        {
            //https://github.com/ExcelDataReader/ExcelDataReader
            var reader = ExcelReaderFactory.CreateReader(stream, new ExcelReaderConfiguration()
            {
                // Gets or sets the encoding to use when the input XLS lacks a CodePage
                // record, or when the input CSV lacks a BOM and does not parse as UTF8. 
                // Default: cp1252 (XLS BIFF2-5 and CSV only)
                FallbackEncoding = Encoding.GetEncoding(1252),
                LeaveOpen = false,
            });
            return reader.AsDataSet();
        }
    }
}
