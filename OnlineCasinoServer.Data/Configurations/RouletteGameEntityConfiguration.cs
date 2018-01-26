using OnlineCasinoServer.Data.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineCasinoServer.Data.Configurations
{
    public class RouletteGameEntityConfiguration : EntityTypeConfiguration<RouletteGame>
    {
        public RouletteGameEntityConfiguration()
        {
            this.HasKey<int>(b => b.Id);
            
            this.Property(p => p.Id)
                .HasDatabaseGeneratedOption(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.Identity);

            this.Property(p => p.MinStake)
                .IsRequired()
                .HasColumnType("Money");

            this.Property(p => p.MaxStake)
                .IsRequired()
                .HasColumnType("Money");

            this.Property(p => p.PlayerCount)
                .IsRequired();

            this.Property(p => p.MaxPlayers)
                .IsRequired();

            this.Property(p => p.LastSpinId)
                .IsRequired();

            this.Property(p => p.BetsCount)
                .IsRequired();
        }
    }
}
