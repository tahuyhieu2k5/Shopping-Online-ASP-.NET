using Microsoft.EntityFrameworkCore;
using Shopping_Tutorial.Models;

namespace Shopping_Tutorial.Repository
{
	public class SeedData
	{
		public static void SeedingData(DataContext _context)
		{
			_context.Database.Migrate();
			if (!_context.Products.Any())
			{
				CategoryModel macbook = new CategoryModel { Name = "Macbook", Slug = "Macbook", Description = "Macbook is brand in the world", Status = 1 };

				CategoryModel pc = new CategoryModel { Name = "pc", Slug = "pc", Description = "pc is brand in the world", Status = 1 };

				BrandModel apple = new BrandModel { Name = "apple", Slug = "apple", Description = "apple is brand in the world", Status = 1 };

				BrandModel samsung = new BrandModel { Name = "samsung", Slug = "samsung", Description = "samsung is brand in the world", Status = 1 };

				_context.Products.AddRange(
					new ProductModel { Name = "Macbook", Slug = "Macbook", Description = "Macbook is the best ", Image = "1.jpg", Category = macbook, Brand = apple, Price = 20000 },

				new ProductModel { Name = "pc", Slug = "pc", Description = "pc is the best ", Image = "1.jpg", Category = pc, Brand = samsung, Price = 30000 }
				);
				_context.SaveChanges();
			}
		}
	}
}
