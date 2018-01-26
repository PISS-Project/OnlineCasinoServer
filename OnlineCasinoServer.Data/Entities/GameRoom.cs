using System.Collections.Generic;

namespace OnlineCasinoServer.Data.Entities
{
    public class GameRoom
    {
        public int Id { get; set; }
        public int GameId { get; set; }
        public string GameType { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public virtual ICollection<GameRoomPlayer> Players { get; set; }
    }
}
