using System.Collections.Generic;

namespace OnlineCasinoServer.Data.Entities
{
    public class RouletteGame
    {
        public int Id { get; set; }
        public decimal MinStake { get; set; }
        public decimal MaxStake { get; set; }
        public int PlayerCount { get; set; }
        public int MaxPlayers { get; set; }
        public int LastSpinId { get; set; }
        public int BetsCount { get; set; }
        public virtual ICollection<RouletteBet> RouletteBets { get; set; }
        public virtual ICollection<RouletteSpin> RouletteSpins { get; set; }
    }
}
