namespace ConnectFourSpel.DAL
{
    public class UserDetails
    {
        public int Id { get; set; }
        public string Username { get; set; } = "";
        public string PasswordHash { get; set; } = "";
    }
}
