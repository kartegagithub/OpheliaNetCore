using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ophelia.Data.Exporter.Controls;

namespace Ophelia.Data.Exporter
{
    public class XMLExporter : IExporter
    {
        public byte[] Export(List<Grid> grids)
        {
            var sb = new StringBuilder();
            sb.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            sb.Append("<documents>");
            foreach (var grid in grids)
            {
                sb.Append(GetFileContent(grid));
            }
            sb.Append("</documents>");
            return System.Text.Encoding.UTF8.GetBytes(sb.ToString());
        }

        public byte[] Export(Grid grid)
        {
            var grids = new List<Grid>();
            grids.Add(grid);
            return this.Export(grids);
        }

        private static string GetFileContent(Controls.Grid grid)
        {
            var doc = new System.Xml.XmlDocument();
            var root = doc.AppendChild(doc.CreateElement("document"));
            root.Attributes.Append(doc.CreateAttribute("text")).InnerText = grid.Text;

            foreach (var row in grid.Rows)
            {
                var xmlRow = root.AppendChild(doc.CreateElement("row"));
                foreach (var cell in row.Cells)
                {
                    var xmlCell = xmlRow.AppendChild(doc.CreateElement(cell.Column.ID));
                    xmlCell.InnerText = Convert.ToString(cell.Value);
                }
            }            
            return doc.OuterXml;
        }
    }
}
