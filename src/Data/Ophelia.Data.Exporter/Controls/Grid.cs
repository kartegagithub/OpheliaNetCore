using System;
using System.Collections.Generic;

namespace Ophelia.Data.Exporter.Controls
{
    public class Grid : IDisposable
    {
        public string ID { get; set; }
        public string Text { get; set; }
        public List<Row> Rows { get; set; }
        public List<Column> Columns { get; set; }
        public long TableStyleID { get; set; }
        public bool IsFilterable { get; set; }
        public bool IsTotalRowShow { get; set; }
        public Grid()
        {
            this.Rows = new List<Row>();
            this.Columns = new List<Column>();
            this.IsFilterable = true;
        }
        public Row AddRow()
        {
            var row = new Row(this);
            this.Rows.Add(row);
            return row;
        }
        public Column AddColumn()
        {
            var column = new Column(this);
            this.Columns.Add(column);
            return column;
        }
        public virtual void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            this.Rows.Clear();
            this.Columns.Clear();
            this.Text = "";
        }
    }
}
