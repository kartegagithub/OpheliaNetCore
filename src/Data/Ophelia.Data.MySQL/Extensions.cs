using System;

namespace Ophelia.Data.MySQL
{
    public static class Extensions
    {
        public static void UseMySQL()
        {
            if (!Ophelia.Data.Connection.ConnectionProviders.ContainsKey("MySQL"))
                Ophelia.Data.Connection.ConnectionProviders.Add("MySQL", typeof(MySql.Data.MySqlClient.MySqlConnection));
        }
    }
}
