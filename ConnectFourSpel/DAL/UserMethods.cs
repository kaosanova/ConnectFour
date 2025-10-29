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
        public static bool Update(int id, string username, string passwordHash)
        {
            using var conn = Db.Open();
            using var cmd = new SqlCommand(@"
UPDATE dbo.Users
SET Username = @u,
    PasswordHash = @p
WHERE Id = @id;", conn);

            cmd.Parameters.AddWithValue("@u", username);
            cmd.Parameters.AddWithValue("@p", passwordHash);
            cmd.Parameters.AddWithValue("@id", id);

            int affectedRows = cmd.ExecuteNonQuery();
            return affectedRows > 0;
        }
        public static UserDetails? GetById(int id)
        {
            using var conn = Db.Open();
            using var cmd = new SqlCommand(
                "SELECT Id, Username, PasswordHash FROM dbo.Users WHERE Id = @id;",
                conn);
            cmd.Parameters.AddWithValue("@id", id);

            using var r = cmd.ExecuteReader();
            if (!r.Read()) return null;

            return new UserDetails
            {
                Id = (int)r["Id"],
                Username = (string)r["Username"],
                PasswordHash = (string)r["PasswordHash"]
            };
        }
        public static bool Delete(int id)
        {
            using var conn = Db.Open();
            using var cmd = new SqlCommand(@"
DELETE FROM dbo.Users
WHERE Id = @id;", conn);

            cmd.Parameters.AddWithValue("@id", id);
            var affected = cmd.ExecuteNonQuery();
            return affected > 0;
        }

    }
}
