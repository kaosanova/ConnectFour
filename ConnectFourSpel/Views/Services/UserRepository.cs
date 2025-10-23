using Microsoft.Data.SqlClient;
using ConnectFourSpel.Models;

namespace ConnectFourSpel.Services;

public interface IUserRepository
{
    Task<User?> GetByUsernameAsync(string username);
    Task AddUserAsync(User user);
}

public class UserRepository : IUserRepository
{
    private readonly IDbConnectionFactory _factory;
    public UserRepository(IDbConnectionFactory factory) => _factory = factory;

    public async Task<User?> GetByUsernameAsync(string username)
    {
        await using var conn = _factory.CreateConnection(); // SqlConnection
        await conn.OpenAsync();

        await using var cmd = new SqlCommand(
            "SELECT Id, Username, PasswordHash FROM Users WHERE Username = @u", conn);
        cmd.Parameters.AddWithValue("@u", username);

        await using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new User
            {
                Id = reader.GetInt32(0),
                Username = reader.GetString(1),
                PasswordHash = reader.GetString(2)
            };
        }
        return null;
    }

    public async Task AddUserAsync(User user)
    {
        await using var conn = _factory.CreateConnection();
        await conn.OpenAsync();

        await using var cmd = new SqlCommand(
            "INSERT INTO Users (Username, PasswordHash) VALUES (@u, @p)", conn);
        cmd.Parameters.AddWithValue("@u", user.Username);
        cmd.Parameters.AddWithValue("@p", user.PasswordHash);

        await cmd.ExecuteNonQueryAsync();
    }
}
