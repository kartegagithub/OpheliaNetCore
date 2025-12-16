namespace Ophelia.Data.SQLServer
{
    public static class Extensions
    {
        private static object LockObj = new object();
        public static void UseOracle()
        {
            lock (LockObj)
            {
                if (!Connection.ConnectionProviders.ContainsKey("Oracle"))
                    Connection.ConnectionProviders.TryAdd("Oracle", typeof(Oracle.ManagedDataAccess.Client.OracleConnection));
            }
        }

        public static DatabaseType UseOracle(this DataContext context)
        {
            UseOracle();
            return DatabaseType.Oracle;
        }
    }
}
