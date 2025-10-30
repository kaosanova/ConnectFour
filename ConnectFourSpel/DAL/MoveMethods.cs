using Microsoft.Data.SqlClient;

namespace ConnectFourSpel.DAL
{
    public static class MoveMethods
    {
        public static MoveDetails Add(int gameId, int playerNo, int col, int row)
        {
            using var conn = Db.Open();

            var nextNoCmd = new SqlCommand("SELECT ISNULL(MAX(MoveNo),0)+1 FROM dbo.tbl_Move WHERE GameId=@g;", conn);
            nextNoCmd.Parameters.AddWithValue("@g", gameId);
            var moveNo = (int)nextNoCmd.ExecuteScalar();

            using var cmd = new SqlCommand(@"
INSERT INTO dbo.tbl_Move (GameId, MoveNo, PlayerNo, Col, Row)
OUTPUT INSERTED.Id, INSERTED.GameId, INSERTED.MoveNo, INSERTED.PlayerNo, INSERTED.Col, INSERTED.Row, INSERTED.MadeAt
VALUES (@g, @n, @p, @c, @r);", conn);

            cmd.Parameters.AddWithValue("@g", gameId);
            cmd.Parameters.AddWithValue("@n", moveNo);
            cmd.Parameters.AddWithValue("@p", playerNo);
            cmd.Parameters.AddWithValue("@c", col);
            cmd.Parameters.AddWithValue("@r", row);

            using var r = cmd.ExecuteReader();
            r.Read();
            return new MoveDetails
            {
                Id = (int)r["Id"],
                GameId = (int)r["GameId"],
                MoveNo = (int)r["MoveNo"],
                PlayerNo = (byte)r["PlayerNo"],
                Col = (int)r["Col"],     // int
                Row = (int)r["Row"],     // int
                MadeAt = (DateTime)r["MadeAt"]
            };
        }

        public static List<MoveDetails> ListByGame(int gameId)
        {
            using var conn = Db.Open();
            using var cmd = new SqlCommand(@"
SELECT Id, GameId, MoveNo, PlayerNo, Col, Row, MadeAt
FROM dbo.tbl_Move
WHERE GameId=@g
ORDER BY MoveNo;", conn);

            cmd.Parameters.AddWithValue("@g", gameId);

            using var r = cmd.ExecuteReader();
            var list = new List<MoveDetails>();
            while (r.Read())
            {
                list.Add(new MoveDetails
                {
                    Id = (int)r["Id"],
                    GameId = (int)r["GameId"],
                    MoveNo = (int)r["MoveNo"],
                    PlayerNo = (byte)r["PlayerNo"],
                    Col = (int)r["Col"],
                    Row = (int)r["Row"],
                    MadeAt = (DateTime)r["MadeAt"]
                });
            }
            return list;
        }
    }
}