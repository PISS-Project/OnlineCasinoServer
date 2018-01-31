using OnlineCasinoServer.Data.Configurations;
using OnlineCasinoServer.Data.Entities;
using System.Data.Entity;

namespace OnlineCasinoServer.Data.Models
{
    public partial class OnlineCasinoDb : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Login> Logins { get; set; }
        public DbSet<DiceBet> DiceBets { get; set; }
        public DbSet<RouletteBet> RouletteBets { get; set; }

        public OnlineCasinoDb()
            : base("OnlineCasinoDb")
        {
        }        

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Configurations.Add(new UserEntityConfiguration());
            modelBuilder.Configurations.Add(new LoginEntityConfiguration());
            modelBuilder.Configurations.Add(new DiceBetEntityConfiguration());
            modelBuilder.Configurations.Add(new RouletteBetEntityConfiguration());

            base.OnModelCreating(modelBuilder);
        }
    }
}
