using System;

namespace Ophelia.Data.Exporter.Controls
{
    public class Cell : IDisposable
    {
        public object Value { get; set; }
        public Grid Grid { get; private set; }
        public Column Column { get; private set; }
        public int ColumnSpan { get; set; }
        public int RowSpan { get; set; }
        public Row Row { get; private set; }
        public Cell(Grid grid, Column column, Row row)
        {
            this.Grid = grid;
            this.Column = column;
            this.Row = row;
        }

        public virtual void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            this.Row = null;
            this.Column = null;
            this.Grid = null;
            this.Value = null;
        }
    }
}
