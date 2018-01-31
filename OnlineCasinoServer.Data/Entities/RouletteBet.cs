using System;

namespace OnlineCasinoServer.Data.Entities
{
    public class RouletteBet
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public virtual User User { get; set; }
        public string BetData { get; set; }
        public int SpinResult { get; set; }
        public decimal Stake { get; set; }
        public decimal Win { get; set; }
        public DateTime CreationDate { get; set; }

        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public int[] BetValues
        {
            get
            {
                return Array.ConvertAll(BetData.Split(','), int.Parse);
            }
            set
            {
                BetData = String.Join(",", value);
            }
        }
    }
}
