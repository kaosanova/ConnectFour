namespace ConnectFourSpel.DAL
{
    public class GameDetails
    {
        public int Id { get; set; }
        public int Player1Id { get; set; }
        public int? Player2Id { get; set; }
        public byte Status { get; set; }         // 0=Active,1=Finished,2=Aborted
        public int? WinnerUserId { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? FinishedAt { get; set; }
    }

    // Praktiskt när ni vill visa allt på en sida
    public class GameWithMoves
    {
        public GameDetails Game { get; set; } = new();
        public List<MoveDetails> Moves { get; set; } = new();
    }
}