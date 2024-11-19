using Microsoft.AspNetCore.Mvc;
using HtmlAgilityPack;  

namespace XcentiumCodeTest.Controllers
{
    public class ScraperController : Controller
    {
        public IActionResult Scraper()
        {
            return View();
        }
        [HttpPost]
        public async Task<JsonResult> ScrapeUrl(string targetUrl)
        {
            if (string.IsNullOrEmpty(targetUrl))
                return Json(new { success = false, message = "URL cannot be empty." });

            try
            {
                // Fetch HTML content
                var client = new HttpClient();
                var html = await client.GetStringAsync(targetUrl);

                // Parse HTML using HtmlAgilityPack
                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                // Scrape images
                var images = new List<string>();
                foreach (var img in doc.DocumentNode.SelectNodes("//img[@src]"))
                {
                    var src = img.GetAttributeValue("src", string.Empty);
                    if (!string.IsNullOrEmpty(src))
                    {
                        // Make sure to create an absolute URL if src is relative
                        Uri baseUri = new Uri(targetUrl);
                        Uri fullUri = new Uri(baseUri, src);
                        images.Add(fullUri.ToString());
                    }
                }

                // Scrape words and count frequency
                var wordFrequency = new Dictionary<string, int>();
                var textContent = doc.DocumentNode.InnerText.ToLower();
                var words = textContent.Split(new[] { ' ', '\n', '\r', '\t', '.', ',', '!', '?', ';' }, StringSplitOptions.RemoveEmptyEntries);

                Console.WriteLine(words);

                foreach (var word in words)
                {
                    if (wordFrequency.ContainsKey(word))
                    {
                        wordFrequency[word]++;
                    }
                    else
                    {
                        wordFrequency[word] = 1;
                    }
                }

                // Get top 10 most frequent words
                var topWords = wordFrequency
                    .OrderByDescending(w => w.Value) // Sort by frequency (highest first)
                    .Take(10) // Get the top 10 words
                    .Select(w => new { Word = w.Key, Count = w.Value }) // Format into anonymous object
                    .ToList();

                // Return result as JSON
                return Json(new
                {
                    success = true,
                    images = images,
                    totalWords = words.Length,
                    topWords = topWords
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error scraping the URL: " + ex.Message });
            }
        }
    }
}

