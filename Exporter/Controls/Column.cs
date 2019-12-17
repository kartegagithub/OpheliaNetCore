using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ophelia.Data.Exporter.Controls
{
    public class Column:IDisposable
    {
        public bool IsNumeric { get; set; }
        public string ID { get; set; }
        public string Text { get; set; }
        public Grid Grid { get; private set; }
        public Column(Grid grid)
        {
            this.Grid = grid;
        }

        public void Dispose()
        {
            this.Grid = null;
            this.Text = "";
            this.ID = "";
        }
    }
}
