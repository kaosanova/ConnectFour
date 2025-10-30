using Microsoft.Data.SqlClient;

namespace ConnectFourSpel.DAL
{
    public static class Db
    {
        private static IConfiguration? _config;
        public static void Configure(IConfiguration config) => _config = config;

        public static SqlConnection Open()
        {
            if (_config == null) throw new InvalidOperationException("Db.Configure saknas.");
            var cs = _config.GetConnectionString("Default")
                     ?? throw new InvalidOperationException("ConnectionStrings:Default saknas.");
            var conn = new SqlConnection(cs);
            conn.Open();
            return conn;
        }
    }
}
