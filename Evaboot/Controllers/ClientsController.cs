using Evaboot.DbContexts;
using Evaboot.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClosedXML.Excel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.InkML;

namespace Evaboot.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class ClientsController : ControllerBase
	{
		private readonly ApplicationDbContext _context;

		public ClientsController(ApplicationDbContext context)
		{
			_context = context;
		}

		[HttpGet]
		public async Task<ActionResult<IEnumerable<Client>>> GetClients()
		{
			return await _context.Clients.ToListAsync();
		}


		[HttpPost("scrape")]
		public async Task<ActionResult<Client>> ScrapeAndAddClient([FromBody] string linkedInUrl)
		{
			var scraper = new LinkedInScraper(_context);
			var client = await scraper.ScrapeProfileData(linkedInUrl);

			return CreatedAtAction(nameof(GetClients), new { id = client.Id }, client);
		}
		[HttpGet("{id}")]
		public async Task<ActionResult<Client>> GetClientById(int id)
		{
			var client = await _context.Clients.FindAsync(id);
			if (client == null)
			{
				return NotFound();
			}
			return client;
		}
		[HttpPut("{id}")]
		public async Task<IActionResult> UpdateClient(int id, Client updatedClient)
		{
			var client = await _context.Clients.FindAsync(id);
			if (client == null)
			{
				return NotFound();
			}

			client.Name = updatedClient.Name;
			client.Email = updatedClient.Email;
			client.Company = updatedClient.Company;
			client.LinkedInUrl = updatedClient.LinkedInUrl;

			_context.Entry(client).State = EntityState.Modified;
			await _context.SaveChangesAsync();

			return NoContent();
		}

		[HttpDelete("{id}")]
		public async Task<IActionResult> DeleteClient(int id)
		{
			var client = await _context.Clients.FindAsync(id);
			if (client == null)
			{
				return NotFound();
			}

			_context.Clients.Remove(client);
			await _context.SaveChangesAsync();

			return NoContent();
		}

		[HttpGet("search")]
		public async Task<ActionResult<IEnumerable<Client>>> SearchClients([FromQuery] string name, [FromQuery] string company)
		{
			var query = _context.Clients.AsQueryable();

			if (!string.IsNullOrEmpty(name))
			{
				query = query.Where(c => c.Name.Contains(name));
			}

			if (!string.IsNullOrEmpty(company))
			{
				query = query.Where(c => c.Company.Contains(company));
			}

			var results = await query.ToListAsync();
			return Ok(results);
		}


		[HttpGet("export")]
		public async Task<IActionResult> ExportToExcel()
		{
			var clients = await _context.Clients.ToListAsync();

			using var workbook = new XLWorkbook();
			var worksheet = workbook.Worksheets.Add("Clients");

			// كتابة الـ Headers
			worksheet.Cell(1, 1).Value = "ID";
			worksheet.Cell(1, 2).Value = "Name";
			worksheet.Cell(1, 3).Value = "LinkedIn URL";
			worksheet.Cell(1, 4).Value = "Email";
			worksheet.Cell(1, 5).Value = "Company";

			// إدخال البيانات داخل الملف
			for (int i = 0; i < clients.Count; i++)
			{
				var client = clients[i];
				worksheet.Cell(i + 2, 1).Value = client.Id;
				worksheet.Cell(i + 2, 2).Value = client.Name;
				worksheet.Cell(i + 2, 3).Value = client.LinkedInUrl;
				worksheet.Cell(i + 2, 4).Value = client.Email;
				worksheet.Cell(i + 2, 5).Value = client.Company;
			}

			// حفظ الملف داخل Stream
			using var stream = new MemoryStream();
			workbook.SaveAs(stream);
			stream.Position = 0;

			// إرجاع الملف للتحميل كـ Response
			return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Clients.xlsx");
		}



	}
}
