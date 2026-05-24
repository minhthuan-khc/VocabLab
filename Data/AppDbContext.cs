using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VocabLab.Models;

namespace VocabLab.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<Word> Words { get; set; }
        public DbSet<UserWordProgress> UserWordProgresses { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Word>().HasData(
                new Word
                {
                    Id = 1,
                    Term = "Apple",
                    Pronounciation = "/ˈæpl/",
                    Definition = "Quả táo",
                    Example = "I eat an apple every day.",
                    Category = "Food"
                },

                new Word
                {
                    Id = 2,
                    Term = "Dog",
                    Pronounciation = "/dɔːɡ/",
                    Definition = "Con chó",
                    Example = "The dog is very friendly.",
                    Category = "Animals"
                }
            );
        }
    }
}