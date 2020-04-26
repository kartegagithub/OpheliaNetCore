using System.Collections.Generic;

namespace Ophelia.Data.Exporter
{
    public interface IExporter
    {
        byte[] Export(Controls.Grid grid);
        byte[] Export(List<Controls.Grid> grids);
    }
}
