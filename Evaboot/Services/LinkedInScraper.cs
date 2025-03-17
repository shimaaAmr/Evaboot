


using Evaboot.DbContexts;
using Evaboot.Models;
using PuppeteerSharp;
using System.Threading.Tasks;

public class LinkedInScraper
{
	private readonly ApplicationDbContext _context;

	public LinkedInScraper(ApplicationDbContext context)
	{
		_context = context;
	}

	public async Task<Client> ScrapeProfileData(string profileUrl)
	{
		await new BrowserFetcher().DownloadAsync(BrowserFetcher.DefaultChromiumRevision);
		await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = true });
		await using var page = await browser.NewPageAsync();
		await page.GoToAsync(profileUrl);

		var nameElement = await page.QuerySelectorAsync("h1");
		var name = await page.EvaluateFunctionAsync<string>("el => el.textContent.trim()", nameElement);

		var companyElement = await page.QuerySelectorAsync(".text-body-medium");
		var company = await page.EvaluateFunctionAsync<string>("el => el.textContent.trim()", companyElement);

		var client = new Client
		{
			Name = name,
			LinkedInUrl = profileUrl,
			Company = company
		};

		_context.Clients.Add(client);
		await _context.SaveChangesAsync();

		return client;
	}
}

