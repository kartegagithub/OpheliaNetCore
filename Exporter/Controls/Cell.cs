using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ophelia.Data.Exporter.Controls
{
    public class Cell: IDisposable
    {
        public object Value { get; set; }
        public Grid Grid { get; private set; }
        public Column Column{ get; private set; }
        public Row Row { get; private set; }
        public Cell(Grid grid, Column column, Row row)
        {
            this.Grid = grid;
            this.Column = column;
            this.Row = row;
        }

        public void Dispose()
        {
            this.Row = null;
            this.Column = null;
            this.Grid = null;
            this.Value = null;
        }
    }
}
