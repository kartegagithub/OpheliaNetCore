using System;

namespace Ophelia.Data.SQLServer
{
    public static class Extensions
    {
        public static void UseOracle()
        {
            if (!Connection.ConnectionProviders.ContainsKey("Oracle"))
                Connection.ConnectionProviders.Add("Oracle", typeof(Oracle.ManagedDataAccess.Client.OracleConnection));
        }

        public static DatabaseType UseOracle(this DataContext context)
        {
            UseOracle();
            return DatabaseType.Oracle;
        }
    }
}
