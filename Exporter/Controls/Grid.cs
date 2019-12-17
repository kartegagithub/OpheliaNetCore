using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ophelia.Data.Exporter.Controls
{
    public class Grid: IDisposable
    {
        public string ID { get; set; }
        public string Text { get; set; }
        public List<Row> Rows { get; set; }
        public List<Column> Columns { get; set; }
        public Grid()
        {
            this.Rows = new List<Row>();
            this.Columns = new List<Column>();
        }

        public void Dispose()
        {
            this.Rows.Clear();
            this.Columns.Clear();
            this.Text = "";
        }
    }
}
