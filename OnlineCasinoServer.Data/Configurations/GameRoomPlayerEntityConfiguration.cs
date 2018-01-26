using OnlineCasinoServer.Data.Entities;
using System.Data.Entity.ModelConfiguration;

namespace OnlineCasinoServer.Data.Configurations
{
    public class GameRoomPlayerEntityConfiguration : EntityTypeConfiguration<GameRoomPlayer>
    {
        public GameRoomPlayerEntityConfiguration()
        {
            this.HasKey(grp => new { grp.GameRoomId, grp.UserId });

            this.HasRequired(grp => grp.GameRoom)
                .WithMany(gr => gr.Players)
                .HasForeignKey(grp => grp.GameRoomId);

            this.HasRequired(grp => grp.User)
                .WithOptional(u => u.GameRoomPlayer);
        }
    }
}
