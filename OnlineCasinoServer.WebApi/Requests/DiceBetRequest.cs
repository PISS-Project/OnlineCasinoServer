using System.ComponentModel.DataAnnotations;

namespace OnlineCasinoServer.WebApi.Requests
{
    public class DiceBetRequest
    {
        [Required]
        [Range(2, 12)]
        public int Bet { get; set; }
        [Required]
        [Range(0.01, double.PositiveInfinity)]
        public decimal Stake { get; set; }
    }
}