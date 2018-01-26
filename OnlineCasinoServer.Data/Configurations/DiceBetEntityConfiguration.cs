using OnlineCasinoServer.Data.Entities;
using System.Data.Entity.ModelConfiguration;

namespace OnlineCasinoServer.Data.Configurations
{
    public class DiceBetEntityConfiguration : EntityTypeConfiguration<DiceBet>
    {
        public DiceBetEntityConfiguration()
        {
            this.HasKey<int>(b => b.Id);

            this.HasRequired(b => b.User)
                .WithMany(u => u.DiceBets)
                .HasForeignKey(b => b.UserId);

            this.Property(p => p.Id)
                .HasDatabaseGeneratedOption(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.Identity);

            this.Property(p => p.UserId)
                .IsRequired();

            this.Property(p => p.DiceSumBet)
                .IsRequired();

            this.Property(p => p.DiceSumResult)
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
