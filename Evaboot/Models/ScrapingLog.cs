namespace Evaboot.Models
{
	public class ScrapingLog
	{
		public int Id { get; set; }
		public string LinkedInUrl { get; set; }
		public DateTime ScrapedAt { get; set; } = DateTime.UtcNow;
		public string Status { get; set; } // Success or Failed
	}
}
