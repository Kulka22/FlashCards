using Microsoft.EntityFrameworkCore;

namespace FlashCards.Models
{
    public class CardContext : DbContext
    {
        public CardContext(DbContextOptions<CardContext> options)
            : base(options) { }

        public DbSet<Card> Cards { get; set; }
        public DbSet<Category> Categories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Card>()
                .HasOne(card => card.Category)
                .WithMany(category => category.Cards)
                .HasForeignKey(card => card.CategoryId);

            base.OnModelCreating(modelBuilder);
        }
    }
}
