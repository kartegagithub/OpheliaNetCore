using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ophelia.Data.Exporter
{
    public interface IExporter
    {
        byte[] Export(Controls.Grid grid);
        byte[] Export(List<Controls.Grid> grids);
    }
}
