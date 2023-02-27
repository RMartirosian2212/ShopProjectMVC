using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ShopProject_Models;

namespace ShopProject_DataAccess.Data
{
	public class ApplicationDbContext : IdentityDbContext
	{
		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
			: base(options)
		{

		}
		public DbSet<Category> Categories { get; set; }
		public DbSet<Product> Products { get; set; }
		public DbSet<ApplicationType> ApplicationTypes { get; set; }
		public DbSet<ApplicationUser> ApplicationUsers { get; set; }
		public DbSet<InquiryHeader> InquiryHeaders { get; set; }
		public DbSet<InquiryDetail> InquiryDetail { get; set; }
		public DbSet<OrderHeader> OrderHeader{ get; set; }
		public DbSet<OrderDetail> OrderDetail { get; set; }
	}
}
