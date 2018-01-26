using OnlineCasinoServer.Data.Entities;
using System.Data.Entity.ModelConfiguration;

namespace OnlineCasinoServer.Data.Configurations
{
    public class LoginEntityConfiguration : EntityTypeConfiguration<Login>
    {
        public LoginEntityConfiguration()
        {
            this.HasKey<int>(l => l.Id);

            this.HasRequired<User>(l => l.User)
                .WithMany(u => u.Logins)
                .HasForeignKey(l => l.UserId);

            this.Property(p => p.Id)
                .HasDatabaseGeneratedOption(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.Identity);

            this.Property(p => p.Token)
                .IsRequired();
        }
    }
}
