namespace ConnectFourSpel.DAL
{
    public class MoveDetails
    {
        public int Id { get; set; }
        public int GameId { get; set; }
        public int MoveNo { get; set; }   // 1..N
        public byte PlayerNo { get; set; } // 1 eller 2
        public byte Col { get; set; }      // 0..6
        public byte? Row { get; set; }     // 0..5
        public DateTime MadeAt { get; set; }
    }
}