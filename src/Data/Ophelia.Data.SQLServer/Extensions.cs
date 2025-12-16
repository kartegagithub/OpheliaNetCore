namespace Ophelia.Data.SQLServer
{
    public static class Extensions
    {
        private static object LockObj = new object();
        public static void UseSQLServer()
        {
            lock (LockObj)
            {
                if (!Connection.ConnectionProviders.ContainsKey("SQLServer"))
                    Connection.ConnectionProviders.TryAdd("SQLServer", typeof(System.Data.SqlClient.SqlConnection));
            }
        }

        public static DatabaseType UseSQLServer(this DataContext context)
        {
            UseSQLServer();
            return DatabaseType.SQLServer;
        }
    }
}
