using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ophelia.Data.Exporter.Controls
{
    public class Row : IDisposable
    {
        public Grid Grid { get; private set; }
        public List<Cell> Cells { get; set; }
        public Row(Grid grid)
        {
            this.Grid = grid;
            this.Cells = new List<Cell>();
        }

        public void Dispose()
        {
            this.Cells.Clear();
            this.Grid = null;
        }
    }
}
