using OnlineCasinoServer.Data.Entities;
using System.Data.Entity.ModelConfiguration;

namespace OnlineCasinoServer.Data.Configurations
{
    public class RouletteSpinConfiguration : EntityTypeConfiguration<RouletteSpin>
    {
        public RouletteSpinConfiguration()
        {
            this.HasKey(s => new { s.RouletteId, s.SpinId });

            this.HasRequired(s => s.RouletteGame)
                .WithMany(r => r.RouletteSpins)
                .HasForeignKey(s => s.RouletteId);

            this.Property(p => p.SpinResult)
                .IsRequired();
        }
    }
}
