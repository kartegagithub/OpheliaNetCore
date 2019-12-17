using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ophelia.Data
{
    public enum JoinType
    {
        Inner = 0,
        Left = 1,
        Right = 2,
        LeftOuter = 3,
        RightOuter = 4
    }

    public enum CommandType
    {
        None = 0,
        Count = 1,
        Identity = 2
    }

    public enum Constraint
    {
        And = 0,
        Or = 1
    }

    public enum Comparison
    {
        Equal = 0,
        Different = 1,
        Greater = 2,
        Less = 3,
        GreaterAndEqual = 4,
        LessAndEqual = 5,
        In = 6,
        Between = 7,
        StartsWith = 8,
        EndsWith = 9,
        Contains = 10,
        Exists = 11
    }

    public enum DatabaseType
    {
        SQLServer = 0,
        PostgreSQL = 1,
        Oracle = 2,
        MySQL = 3
    }

    public enum EntityState
    {
        Loading = 0,
        Loaded = 1
    }
}
