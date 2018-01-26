namespace OnlineCasinoServer.Data.Entities
{
    public class Login
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public virtual User User { get; set; }
        public string Token { get; set; }
    }
}
