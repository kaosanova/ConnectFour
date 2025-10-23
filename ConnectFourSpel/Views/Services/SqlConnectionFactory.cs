using Microsoft.Data.SqlClient;

namespace ConnectFourSpel.Services;

public interface IDbConnectionFactory
{
    SqlConnection CreateConnection(); // <-- returnerar konkret typ nu
}

public class SqlConnectionFactory : IDbConnectionFactory
{
    private readonly IConfiguration _config;
    public SqlConnectionFactory(IConfiguration config) => _config = config;

    public SqlConnection CreateConnection()
        => new SqlConnection(_config.GetConnectionString("Default"));
}
