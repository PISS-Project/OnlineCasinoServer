using System;

namespace OnlineCasinoServer.Data.Entities
{
    public class RouletteBet
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public virtual User User { get; set; }
        public int RouletteId { get; set; }
        public virtual RouletteGame RouletteGame { get; set; }
        public int SpinId { get; set; }
        public string BetType { get; set; }
        public string BetValue { get; set; }
        public int SpinResult { get; set; }
        public decimal Stake { get; set; }
        public decimal Win { get; set; }
        public DateTime CreationDate { get; set; }
    }
}
