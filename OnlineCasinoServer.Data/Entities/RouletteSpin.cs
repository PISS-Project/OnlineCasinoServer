namespace OnlineCasinoServer.Data.Entities
{
    public class RouletteSpin
    {
        public int RouletteId { get; set; }
        public virtual RouletteGame RouletteGame { get; set; }
        public int SpinId { get; set; }
        public int SpinResult { get; set; }
    }
}
