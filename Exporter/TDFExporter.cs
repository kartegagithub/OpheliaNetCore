using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ophelia.Data.Exporter.Controls;

namespace Ophelia.Data.Exporter
{
    public class TDFExporter : IExporter
    {
        public byte[] Export(List<Grid> grids)
        {
            return System.Text.Encoding.UTF8.GetBytes(GetFileContent(grids.FirstOrDefault()));
        }

        public byte[] Export(Grid grid)
        {
            var grids = new List<Grid>();
            grids.Add(grid);
            return this.Export(grids);
        }

        private static string GetFileContent(Controls.Grid grid)
        {
            var sb = new StringBuilder();

            foreach (var col in grid.Columns)
            {
                sb.Append(col.Text);
                sb.Append("\t");
            }
            sb.Append("\n");

            foreach (var row in grid.Rows)
            {
                foreach (var cell in row.Cells)
                {
                    sb.Append(Convert.ToString(cell.Value));
                    sb.Append("\t");
                }
                sb.Append("\n");
            }
            
            return sb.ToString();
        }
    }
}
