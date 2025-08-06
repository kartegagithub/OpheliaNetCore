using System;
using DocumentFormat.OpenXml.Spreadsheet;

namespace Ophelia.Data.Exporter.Controls
{
    public class Column : IDisposable
    {
        public bool IsNumeric { get; set; }
        public string ID { get; set; }
        public string Text { get; set; }
        public CellValues? Type { get; set; }
        public Type? ValueType { get; set; }
        public Grid Grid { get; private set; }
        public int StyleID { get; set; } = -1;
        public long CalculationTypeID { get; set; }
        public Column(Grid grid)
        {
            this.Grid = grid;
        }

        public virtual void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            this.Grid = null;
            this.Text = "";
            this.ID = "";
        }
    }
}
