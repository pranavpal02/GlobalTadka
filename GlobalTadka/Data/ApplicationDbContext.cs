using GlobalTadka.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace GlobalTadka.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Ingredient> Ingredients { get; set; }
        public DbSet<ProductIngredient> ProductIngredients { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Define composite key and relationships for ProductIngredient
            modelBuilder.Entity<ProductIngredient>()
                .HasKey(pi => new { pi.ProductId, pi.IngredientId });

            modelBuilder.Entity<ProductIngredient>()
                .HasOne(pi => pi.Product)
                .WithMany(p => p.ProductIngredients)
                .HasForeignKey(pi => pi.ProductId);

            modelBuilder.Entity<ProductIngredient>()
                .HasOne(pi => pi.Ingredient)
                .WithMany(i => i.ProductIngredients)
                .HasForeignKey(pi => pi.IngredientId);

            // Seed Data: Gujarati Cuisine Categories
            modelBuilder.Entity<Category>().HasData(
                new Category { CategoryId = 1, Name = "Snacks" },
                new Category { CategoryId = 2, Name = "Main Course" },
                new Category { CategoryId = 3, Name = "Bread" },
                new Category { CategoryId = 4, Name = "Sweets" },
                new Category { CategoryId = 5, Name = "Beverages" }
            );

            // Seed Data: Common Gujarati Ingredients
            modelBuilder.Entity<Ingredient>().HasData(
                new Ingredient { IngredientId = 1, Name = "Besan (Gram Flour)" },
                new Ingredient { IngredientId = 2, Name = "Wheat Flour" },
                new Ingredient { IngredientId = 3, Name = "Jaggery" },
                new Ingredient { IngredientId = 4, Name = "Rice" },
                new Ingredient { IngredientId = 5, Name = "Curd" },
                new Ingredient { IngredientId = 6, Name = "Potato" },
                new Ingredient { IngredientId = 7, Name = "Chili" },
                new Ingredient { IngredientId = 8, Name = "Cumin" },
                new Ingredient { IngredientId = 9, Name = "Turmeric" },
                new Ingredient { IngredientId = 10, Name = "Mustard Seeds" }
            );

            // Seed Data: Gujarati Dishes
            modelBuilder.Entity<Product>().HasData(
                new Product
                {
                    ProductId = 1,
                    Name = "Dhokla",
                    Description = "Soft and fluffy steamed gram flour cake, a Gujarati favorite.",
                    Price = 5.99m,
                    Stock = 50,
                    CategoryId = 1
                },
                new Product
                {
                    ProductId = 2,
                    Name = "Thepla",
                    Description = "Spiced whole wheat flatbread infused with methi (fenugreek).",
                    Price = 4.99m,
                    Stock = 60,
                    CategoryId = 3
                },
                new Product
                {
                    ProductId = 3,
                    Name = "Undhiyu",
                    Description = "A rich and flavorful mixed vegetable dish, cooked with special spices.",
                    Price = 8.99m,
                    Stock = 40,
                    CategoryId = 2
                },
                new Product
                {
                    ProductId = 4,
                    Name = "Shrikhand",
                    Description = "Sweetened hung curd flavored with saffron and cardamom.",
                    Price = 6.49m,
                    Stock = 30,
                    CategoryId = 4
                },
                new Product
                {
                    ProductId = 5,
                    Name = "Masala Chaas",
                    Description = "Refreshing buttermilk spiced with roasted cumin and mint.",
                    Price = 2.99m,
                    Stock = 70,
                    CategoryId = 5
                }
            );

            // Seed Data: Product-Ingredient Relationships
            modelBuilder.Entity<ProductIngredient>().HasData(
                new ProductIngredient { ProductId = 1, IngredientId = 1 }, // Dhokla - Besan
                new ProductIngredient { ProductId = 1, IngredientId = 5 }, // Dhokla - Curd
                new ProductIngredient { ProductId = 1, IngredientId = 10 }, // Dhokla - Mustard Seeds

                new ProductIngredient { ProductId = 2, IngredientId = 2 }, // Thepla - Wheat Flour
                new ProductIngredient { ProductId = 2, IngredientId = 7 }, // Thepla - Chili
                new ProductIngredient { ProductId = 2, IngredientId = 9 }, // Thepla - Turmeric

                new ProductIngredient { ProductId = 3, IngredientId = 6 }, // Undhiyu - Potato
                new ProductIngredient { ProductId = 3, IngredientId = 4 }, // Undhiyu - Rice
                new ProductIngredient { ProductId = 3, IngredientId = 8 }, // Undhiyu - Cumin

                new ProductIngredient { ProductId = 4, IngredientId = 5 }, // Shrikhand - Curd
                new ProductIngredient { ProductId = 4, IngredientId = 3 }, // Shrikhand - Jaggery

                new ProductIngredient { ProductId = 5, IngredientId = 5 }, // Masala Chaas - Curd
                new ProductIngredient { ProductId = 5, IngredientId = 8 }  // Masala Chaas - Cumin
            );
        }

    }

}
