using Microsoft.Data.SqlClient;

namespace ConnectFourSpel.DAL
{
    public static class GameMethods
    {
        // Väg A: skapa spel mot SessionId
        public static int Create(string sessionId)
        {
            using var conn = Db.Open();
            using var cmd = new SqlCommand(@"
INSERT INTO dbo.tbl_Game (SessionId)
OUTPUT INSERTED.Id
VALUES (@sid);", conn);

            cmd.Parameters.AddWithValue("@sid", sessionId);
            return (int)cmd.ExecuteScalar()!;
        }

        public static GameDetails? Get(int gameId)
        {
            using var conn = Db.Open();
            using var cmd = new SqlCommand(@"
SELECT Id, SessionId, CreatedAt
FROM dbo.tbl_Game
WHERE Id=@id;", conn);

            cmd.Parameters.AddWithValue("@id", gameId);

            using var r = cmd.ExecuteReader();
            if (!r.Read()) return null;

            // OBS: tabellen har inte Player1Id/Status/... → sätt rimliga default
            var createdAt = (DateTime)r["CreatedAt"];
            return new GameDetails
            {
                Id = (int)r["Id"],
                Player1Id = 0,            // default – du använder PlayerNo i tbl_Move
                Player2Id = null,
                Status = 0,               // 0=Active
                WinnerUserId = null,
                StartedAt = createdAt,    // mappa CreatedAt -> StartedAt
                FinishedAt = null
            };
        }

        public static GameWithMoves? GetWithMoves(int gameId)
        {
            var g = Get(gameId);
            if (g == null) return null;
            var moves = MoveMethods.ListByGame(gameId);
            return new GameWithMoves { Game = g, Moves = moves };
        }

        // Minimal policy: kasta bort spelet när det är klart
        public static void Delete(int gameId)
        {
            using var conn = Db.Open();
            using var cmd = new SqlCommand("DELETE FROM dbo.tbl_Game WHERE Id=@id;", conn);
            cmd.Parameters.AddWithValue("@id", gameId);
            cmd.ExecuteNonQuery();
        }
    }
}
