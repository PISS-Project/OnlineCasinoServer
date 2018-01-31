using System;

namespace OnlineCasinoServer.Core.DTOs
{
    public class RouletteBetDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int[] BetValues { get; set; }
        public int SpinResult { get; set; }
        public decimal Stake { get; set; }
        public decimal Win { get; set; }
        public DateTime CreationDate { get; set; }
    }
}
