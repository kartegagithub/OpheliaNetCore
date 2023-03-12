using Npgsql;
using System;

namespace Ophelia.Data.Npgsql
{
    public static class Extensions
    {
        public static void UseNpgsql()
        {
            if (!Ophelia.Data.Connection.ConnectionProviders.ContainsKey("Npgsql"))
                Ophelia.Data.Connection.ConnectionProviders.Add("Npgsql", typeof(NpgsqlConnection));
        }
    }
}
