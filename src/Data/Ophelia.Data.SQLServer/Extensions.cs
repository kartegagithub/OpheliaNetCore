using System;

namespace Ophelia.Data.SQLServer
{
    public static class Extensions
    {
        public static void UseSQLServer()
        {
            if (!Ophelia.Data.Connection.ConnectionProviders.ContainsKey("SQLServer"))
                Ophelia.Data.Connection.ConnectionProviders.Add("SQLServer", typeof(System.Data.SqlClient.SqlConnection));
        }
    }
}
