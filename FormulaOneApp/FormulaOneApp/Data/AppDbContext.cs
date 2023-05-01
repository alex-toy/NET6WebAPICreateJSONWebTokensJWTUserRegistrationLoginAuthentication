using FormulaOneApp.Models;
using FormulaOneApp.Models.AuthModels;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FormulaOneApp.Data
{
    public class AppDbContext : IdentityDbContext
    {
        public DbSet<Team> Teams { get; set; }
        public DbSet<Pilot> Pilots { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Team>().HasMany(team => team.Pilots);
        }
    }
}
