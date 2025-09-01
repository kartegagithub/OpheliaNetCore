namespace Ophelia.Data.MySQL
{
    public static class Extensions
    {
        public static void UseMySQL()
        {
            if (!Connection.ConnectionProviders.ContainsKey("MySQL"))
                Connection.ConnectionProviders.Add("MySQL", typeof(MySql.Data.MySqlClient.MySqlConnection));
        }

        public static DatabaseType UseMySQL(this DataContext context)
        {
            UseMySQL();
            return DatabaseType.MySQL;
        }
    }
}
