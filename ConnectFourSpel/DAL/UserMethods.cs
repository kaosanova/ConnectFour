using Microsoft.Data.SqlClient;

namespace ConnectFourSpel.DAL
{
    public static class UserMethods
    {
        public static UserDetails? GetByUsername(string username)
        {
            using var conn = Db.Open();
            using var cmd = new SqlCommand(
                "SELECT Id, Username, PasswordHash FROM dbo.Users WHERE Username = @u;",
                conn);
            cmd.Parameters.AddWithValue("@u", username);

            using var r = cmd.ExecuteReader();
            if (!r.Read()) return null;

            return new UserDetails
            {
                Id = (int)r["Id"],
                Username = (string)r["Username"],
                PasswordHash = (string)r["PasswordHash"]
            };
        }

        public static int Create(string username, string passwordHash)
        {
            using var conn = Db.Open();
            using var cmd = new SqlCommand(@"
INSERT INTO dbo.Users (Username, PasswordHash)
OUTPUT INSERTED.Id
VALUES (@u, @p);", conn);

            cmd.Parameters.AddWithValue("@u", username);
            cmd.Parameters.AddWithValue("@p", passwordHash);

            return (int)cmd.ExecuteScalar();
        }
    }
}
