using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace StarTrekOnline_ServerStatus
{
    public interface INewsProcessor
    {
        Task<List<NewsContent>> GetNewsContents();
    }

    public class NewsContent
    {
        public string? Title { get; set; }
        public string? ImageUrl { get; set; }
        
        public string? NewsLink { get; set; }
    }

    public class NewsProcessor : INewsProcessor
    {
        public async Task<List<NewsContent>> GetNewsContents()
        {
            List<NewsContent> newsContents = new List<NewsContent>();

            string url = "https://www.arcgames.com/en/games/star-trek-online/news";
            string baseUrl = "https://www.arcgames.com";

            try
            {
                HttpClient client = new HttpClient();
                HttpResponseMessage response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    string html = await response.Content.ReadAsStringAsync();
                    HtmlDocument htmlDoc = new HtmlDocument();
                    htmlDoc.LoadHtml(html);

                    // Specify the XPath to select the news content elements containing titles and image URLs.
                    string newsXPath = "//div[contains(@class, 'news-content') and contains(@class, 'element')]";
                    HtmlNodeCollection newsNodes = htmlDoc.DocumentNode.SelectNodes(newsXPath);

                    if (newsNodes != null)
                    {
                        foreach (HtmlNode node in newsNodes)
                        {
                            string? title = node.SelectSingleNode(".//h2[@class='news-title']")?.InnerText?.Trim();
                            string? imageUrl = node.SelectSingleNode(".//img[@class='item-img']")?.GetAttributeValue("src", "");
                            string? newsLink = baseUrl + node.SelectSingleNode(".//a[@class='read-more']")?.GetAttributeValue("href", "");

                            NewsContent newsContent = new NewsContent
                            {
                                Title = title,
                                ImageUrl = imageUrl,
                                NewsLink = newsLink
                            };

                            newsContents.Add(newsContent);
                        }
                    }
                }
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Request error: {e.Message}");
            }

            return newsContents;
        }
    }
}