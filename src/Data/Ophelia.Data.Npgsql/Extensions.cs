using Npgsql;

namespace Ophelia.Data.Npgsql
{
    public static class Extensions
    {
        public static void UseNpgsql()
        {
            if (!Connection.ConnectionProviders.ContainsKey("Npgsql"))
                Connection.ConnectionProviders.Add("Npgsql", typeof(NpgsqlConnection));
        }

        public static DatabaseType UseNpgsql(this DataContext context)
        {
            UseNpgsql();
            return DatabaseType.PostgreSQL;
        }
    }
}
