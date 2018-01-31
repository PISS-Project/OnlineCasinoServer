using System.ComponentModel.DataAnnotations;

namespace OnlineCasinoServer.WebApi.Requests
{
    public class RouletteBetRequest
    {
        [Required]
        [MinLength(1)]
        public int[] BetValues { get; set; }
        [Required]
        [Range(0.01, double.PositiveInfinity)]
        public decimal Stake { get; set; }
    }
}