using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace XcentiumCodeTest.Controllers
{
    public class ImageController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            // Render the main view where users can input a URL
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Scrape(string targetUrl)
        {
            if (string.IsNullOrWhiteSpace(targetUrl))
            {
                return BadRequest("URL cannot be empty.");
            }

            try
            {
                using var httpClient = new HttpClient();
                var html = await httpClient.GetStringAsync(targetUrl);

                var baseUri = new Uri(targetUrl);
                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(html);

                // Extract images and resolve relative URLs
                var images = htmlDoc.DocumentNode
                    .SelectNodes("//img")
                    ?.Select(node => node.GetAttributeValue("src", ""))
                    .Where(src => !string.IsNullOrEmpty(src))
                    .Select(src => new Uri(baseUri, src).ToString()) // Resolve relative URLs
                    .ToList() ?? new List<string>();

                var text = Regex.Replace(htmlDoc.DocumentNode.InnerText, @"\s+", " ").ToLower();
                var wordFrequencies = Regex.Matches(text, @"\b\w+\b")
                    .Cast<Match>()
                    .GroupBy(m => m.Value)
                    .Select(g => new { Word = g.Key, Count = g.Count() })
                    .OrderByDescending(w => w.Count)
                    .Take(10)
                    .ToList();

                return Json(new
                {
                    images,
                    totalWords = wordFrequencies.Sum(w => w.Count),
                    wordFrequencies
                });
            }
            catch
            {
                return BadRequest("Failed to scrape the URL. Please check the URL and try again.");
            }
        }

    }
}


