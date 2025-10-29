using ConnectFourSpel.DAL;
using Microsoft.Data.SqlClient;

namespace ConnectFourSpel.DAL
{
    public static class GameMethods
    {
        public static int Create(int player1Id, int? player2Id = null)
        {
            using var conn = Db.Open();
            using var cmd = new SqlCommand(@"
INSERT INTO tbl_Game (Player1Id, Player2Id, Status, StartedAt)
OUTPUT INSERTED.Id
VALUES (@p1, @p2, 0, SYSUTCDATETIME());", conn);

            cmd.Parameters.AddWithValue("@p1", player1Id);
            cmd.Parameters.AddWithValue("@p2", (object?)player2Id ?? DBNull.Value);

            return (int)cmd.ExecuteScalar();
        }

        public static GameDetails? Get(int gameId)
        {
            using var conn = Db.Open();
            using var cmd = new SqlCommand("SELECT * FROM tbl_Game WHERE Id=@id;", conn);
            cmd.Parameters.AddWithValue("@id", gameId);
            using var r = cmd.ExecuteReader();
            if (!r.Read()) return null;
            return Map(r);
        }

        public static GameWithMoves? GetWithMoves(int gameId)
        {
            var g = Get(gameId);
            if (g == null) return null;
            var moves = MoveMethods.ListByGame(gameId);
            return new GameWithMoves { Game = g, Moves = moves };
        }

        public static List<GameDetails> ListForUser(int userId, int? status = null)
        {
            using var conn = Db.Open();
            var sql = @"SELECT * FROM tbl_Game WHERE (Player1Id=@u OR Player2Id=@u)";
            if (status.HasValue) sql += " AND Status=@s";
            sql += " ORDER BY StartedAt DESC;";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@u", userId);
            if (status.HasValue) cmd.Parameters.AddWithValue("@s", status.Value);

            using var r = cmd.ExecuteReader();
            var list = new List<GameDetails>();
            while (r.Read()) list.Add(Map(r));
            return list;
        }

        public static void Finish(int gameId, int? winnerUserId)
        {
            using var conn = Db.Open();
            using var cmd = new SqlCommand(@"
UPDATE tbl_Game 
SET Status=1, WinnerUserId=@w, FinishedAt=SYSUTCDATETIME()
WHERE Id=@id;", conn);
            cmd.Parameters.AddWithValue("@id", gameId);
            cmd.Parameters.AddWithValue("@w", (object?)winnerUserId ?? DBNull.Value);
            cmd.ExecuteNonQuery();
        }

        public static void Delete(int gameId)
        {
            using var conn = Db.Open();
            using var cmd = new SqlCommand("DELETE FROM tbl_Game WHERE Id=@id;", conn);
            cmd.Parameters.AddWithValue("@id", gameId);
            cmd.ExecuteNonQuery();
        }

        private static GameDetails Map(SqlDataReader r) => new()
        {
            Id = (int)r["Id"],
            Player1Id = (int)r["Player1Id"],
            Player2Id = r["Player2Id"] as int?,
            Status = (byte)r["Status"],
            WinnerUserId = r["WinnerUserId"] as int?,
            StartedAt = (DateTime)r["StartedAt"],
            FinishedAt = r["FinishedAt"] as DateTime?
        };
    }
}