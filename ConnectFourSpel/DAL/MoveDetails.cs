namespace ConnectFourSpel.DAL
{
    public class MoveDetails
    {
        public int Id { get; set; }
        public int GameId { get; set; }
        public int MoveNo { get; set; }
        public byte PlayerNo { get; set; }   // <-- byte 1 eller 2
        public int Col { get; set; }
        public int Row { get; set; }
        public DateTime MadeAt { get; set; }
    }
}