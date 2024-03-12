using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;

namespace Forno.Models
{
    public partial class ModelDbContext : DbContext
    {
        public ModelDbContext()
            : base("name=ModelDbContext")
        {
        }

        public virtual DbSet<AppUser> AppUser { get; set; }
        public virtual DbSet<Ingredient> Ingredient { get; set; }
        public virtual DbSet<OrderDetail> OrderDetail { get; set; }
        public virtual DbSet<Orderr> Orderr { get; set; }
        public virtual DbSet<Product> Product { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AppUser>()
                .HasMany(e => e.Orderr)
                .WithRequired(e => e.AppUser)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Ingredient>()
                .HasMany(e => e.Product)
                .WithMany(e => e.Ingredient)
                .Map(m => m.ToTable("ProductIngredient").MapLeftKey("IngredientID").MapRightKey("ProductID"));

            modelBuilder.Entity<Orderr>()
                .Property(e => e.TotalPrice)
                .HasPrecision(10, 2);

            modelBuilder.Entity<Orderr>()
                .HasMany(e => e.OrderDetail)
                .WithRequired(e => e.Orderr)
                .HasForeignKey(e => e.OrderrID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Product>()
                .Property(e => e.Price)
                .HasPrecision(10, 2);

            modelBuilder.Entity<Product>()
                .HasMany(e => e.OrderDetail)
                .WithRequired(e => e.Product)
                .WillCascadeOnDelete(false);
        }
    }
}
