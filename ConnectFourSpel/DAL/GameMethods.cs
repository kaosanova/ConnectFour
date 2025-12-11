using Microsoft.Data.SqlClient;

namespace ConnectFourSpel.DAL
{
    public static class GameMethods
    {
        // Skapa nytt spel där player1Id är den inloggade användaren
        public static int Create(int player1Id)
        {
            using var conn = Db.Open();
            using var cmd = new SqlCommand(@"
INSERT INTO dbo.tbl_Game (Player1Id, Status, StartedAt)
OUTPUT INSERTED.Id
VALUES (@p1, 1, SYSDATETIME());", conn);

            cmd.Parameters.AddWithValue("@p1", player1Id);
            return (int)cmd.ExecuteScalar()!;
        }

        // Lägg till player2 om plats finns och det inte är samma som player1
        public static bool AddPlayer2(int gameId, int player2Id)
        {
            using var conn = Db.Open();
            using var cmd = new SqlCommand(@"
UPDATE dbo.tbl_Game
SET Player2Id = @p2
WHERE Id = @id AND Player2Id IS NULL AND Player1Id <> @p2;", conn);

            cmd.Parameters.AddWithValue("@id", gameId);
            cmd.Parameters.AddWithValue("@p2", player2Id);

            return cmd.ExecuteNonQuery() == 1;
        }

        // Sätter vinnare och markerar spelet som färdigt
        public static void SetWinner(int gameId, int winnerUserId)
        {
            using var conn = Db.Open();
            using var cmd = new SqlCommand(@"
UPDATE dbo.tbl_Game
SET WinnerUserId = @w,
    Status       = 2,
    FinishedAt   = SYSDATETIME()
WHERE Id = @id;", conn);

            cmd.Parameters.AddWithValue("@w", winnerUserId);
            cmd.Parameters.AddWithValue("@id", gameId);
            cmd.ExecuteNonQuery();
        }

        // Hämta ett spel
        public static GameDetails? Get(int gameId)
        {
            using var conn = Db.Open();
            using var cmd = new SqlCommand(@"
SELECT Id,
       Player1Id,
       Player2Id,
       Status,
       WinnerUserId,
       StartedAt,
       FinishedAt
FROM dbo.tbl_Game
WHERE Id = @id;", conn);

            cmd.Parameters.AddWithValue("@id", gameId);

            using var r = cmd.ExecuteReader();
            if (!r.Read()) return null;

            int ordPlayer2 = r.GetOrdinal("Player2Id");
            int ordWinner = r.GetOrdinal("WinnerUserId");
            int ordFinished = r.GetOrdinal("FinishedAt");

            return new GameDetails
            {
                Id = (int)r["Id"],
                Player1Id = (int)r["Player1Id"],
                Player2Id = r.IsDBNull(ordPlayer2) ? (int?)null : r.GetInt32(ordPlayer2),
                Status = (byte)r["Status"],
                WinnerUserId = r.IsDBNull(ordWinner) ? (int?)null : r.GetInt32(ordWinner),
                StartedAt = (DateTime)r["StartedAt"],
                FinishedAt = r.IsDBNull(ordFinished) ? (DateTime?)null : r.GetDateTime(ordFinished)
            };
        }

        // Hämta spel + alla drag
        public static GameWithMoves? GetWithMoves(int gameId)
        {
            var g = Get(gameId);
            if (g == null) return null;

            var moves = MoveMethods.ListByGame(gameId);
            return new GameWithMoves
            {
                Game = g,
                Moves = moves
            };
        }
    }
}
