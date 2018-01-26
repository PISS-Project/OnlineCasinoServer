using OnlineCasinoServer.Data.Entities;
using System.Data.Entity.ModelConfiguration;

namespace OnlineCasinoServer.Data.Configurations
{
    public class RouletteBetEntityConfiguration : EntityTypeConfiguration<RouletteBet>
    {
        public RouletteBetEntityConfiguration()
        {
            this.HasKey<int>(b => b.Id);

            this.HasRequired(b => b.User)
                .WithMany(u => u.RouletteBets)
                .HasForeignKey(b => b.UserId);

            this.HasRequired(b => b.RouletteGame)
                .WithMany(r => r.RouletteBets)
                .HasForeignKey(b => b.RouletteId);

            this.Property(p => p.Id)
                .HasDatabaseGeneratedOption(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.Identity);

            this.Property(p => p.UserId)
                .IsRequired();

            this.Property(p => p.RouletteId)
                .IsRequired();

            this.Property(p => p.SpinId)
                .IsRequired();

            this.Property(p => p.BetType)
                .IsRequired();

            this.Property(p => p.BetValue)
                .IsRequired();

            this.Property(p => p.SpinResult)
                .IsRequired();

            this.Property(p => p.Stake)
                .IsRequired()
                .HasColumnType("Money");

            this.Property(p => p.Win)
                .IsRequired()
                .HasColumnType("Money");

            this.Property(p => p.CreationDate)
                .IsRequired()
                .HasColumnType("DateTime2");
        }
    }
}
