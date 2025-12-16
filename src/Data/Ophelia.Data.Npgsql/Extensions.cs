using Npgsql;

namespace Ophelia.Data.Npgsql
{
    public static class Extensions
    {
        private static object LockObj = new object();
        public static void UseNpgsql()
        {
            lock (LockObj)
            {
                if (!Connection.ConnectionProviders.ContainsKey("Npgsql"))
                    Connection.ConnectionProviders.TryAdd("Npgsql", typeof(NpgsqlConnection));
            }
        }

        public static DatabaseType UseNpgsql(this DataContext context)
        {
            UseNpgsql();
            return DatabaseType.PostgreSQL;
        }
    }
}
