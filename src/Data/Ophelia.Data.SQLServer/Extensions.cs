using System;

namespace Ophelia.Data.SQLServer
{
    public static class Extensions
    {
        public static void UseSQLServer()
        {
            if (!Connection.ConnectionProviders.ContainsKey("SQLServer"))
                Connection.ConnectionProviders.Add("SQLServer", typeof(System.Data.SqlClient.SqlConnection));
        }

        public static DatabaseType UseSQLServer(this DataContext context)
        {
            UseSQLServer();
            return DatabaseType.SQLServer;
        }
    }
}
