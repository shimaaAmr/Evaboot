using Evaboot.Models;
using Microsoft.EntityFrameworkCore;

namespace Evaboot.DbContexts
{
	public class ApplicationDbContext : DbContext
	{
		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

		public DbSet<Client> Clients { get; set; }
		public DbSet<ScrapingLog> ScrapingLogs { get; set; }


	}
}
