using OnlineCasinoServer.Data.Entities;
using System.Data.Entity.ModelConfiguration;

namespace OnlineCasinoServer.Data.Configurations
{
    public class UserEntityConfiguration : EntityTypeConfiguration<User>
    {
        public UserEntityConfiguration()
        {
            this.HasKey<int>(u => u.Id);

            this.Property(p => p.Id)
                .HasDatabaseGeneratedOption(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.Identity);

            this.Property(p => p.Username)
                .IsRequired();

            this.Property(p => p.Password)
                .IsRequired();

            this.Property(p => p.Salt)
                .IsRequired();

            this.Property(p => p.FullName)
                .IsRequired();

            this.Property(p => p.Email)
                .IsRequired();

            this.Property(p => p.Money)
                .IsRequired()
                .HasColumnType("Money");
        }
    }
}
