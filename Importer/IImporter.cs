using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ophelia.Data.Importer
{
    public interface IImporter
    {
        bool FirstRowIsHeader { get; set; }
        Dictionary<int, string> ColumnMappings { get; set; }
        List<TResult> Import<TResult>(System.IO.Stream stream);
        System.Data.DataSet Import(System.IO.Stream stream);

        List<Ophelia.Data.Exporter.Controls.Grid> ImportAsGrid(System.IO.Stream stream);
    }
}
