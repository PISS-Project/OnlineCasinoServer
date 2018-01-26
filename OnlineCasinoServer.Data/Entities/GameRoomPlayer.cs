namespace OnlineCasinoServer.Data.Entities
{
    public class GameRoomPlayer
    {
        public int GameRoomId { get; set; }
        public virtual GameRoom GameRoom { get; set; }
        public int UserId { get; set; }
        public virtual User User { get; set; }
    }
}
