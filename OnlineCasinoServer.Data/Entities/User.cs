using System.Collections.Generic;

namespace OnlineCasinoServer.Data.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Salt { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public decimal Money { get; set; }
        public virtual ICollection<Login> Logins { get; set; }
        public virtual ICollection<DiceBet> DiceBets { get; set; }
        public virtual ICollection<RouletteBet> RouletteBets { get; set; }
        public virtual GameRoomPlayer GameRoomPlayer { get; set; }

        public User() { }
    }
}
