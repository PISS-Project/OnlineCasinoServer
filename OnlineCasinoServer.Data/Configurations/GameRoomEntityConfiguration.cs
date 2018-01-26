using OnlineCasinoServer.Data.Entities;
using System.Data.Entity.ModelConfiguration;

namespace OnlineCasinoServer.Data.Configurations
{
    public class GameRoomEntityConfiguration : EntityTypeConfiguration<GameRoom>
    {
        public GameRoomEntityConfiguration()
        {
            this.HasKey<int>(b => b.Id);

            this.Property(p => p.Id)
                .HasDatabaseGeneratedOption(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.Identity);

            this.Property(p => p.GameId)
                .IsRequired();

            this.Property(p => p.GameType)
                .IsRequired();

            this.Property(p => p.Name)
                .IsRequired();
        }
    }
}
