using System;

namespace Ophelia.Data.SQLServer
{
    public static class Extensions
    {
        public static void UseOracle()
        {
            if (!Ophelia.Data.Connection.ConnectionProviders.ContainsKey("Oracle"))
                Ophelia.Data.Connection.ConnectionProviders.Add("Oracle", typeof(Oracle.ManagedDataAccess.Client.OracleConnection));
        }
    }
}
