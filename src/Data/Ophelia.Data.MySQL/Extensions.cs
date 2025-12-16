namespace Ophelia.Data.MySQL
{
    public static class Extensions
    {
        private static object LockObj = new object();
        public static void UseMySQL()
        {
            lock (LockObj)
            {
                if (!Connection.ConnectionProviders.ContainsKey("MySQL"))
                    Connection.ConnectionProviders.TryAdd("MySQL", typeof(MySql.Data.MySqlClient.MySqlConnection));
            }
        }

        public static DatabaseType UseMySQL(this DataContext context)
        {
            UseMySQL();
            return DatabaseType.MySQL;
        }
    }
}
