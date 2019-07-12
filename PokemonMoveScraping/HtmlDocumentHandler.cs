using HtmlAgilityPack;
using System;

namespace Fraser.GenericMethods
{
    public class HtmlDocumentHandler
    {
        private static DateTime timeOfLastScrape;
        private const int minimumTimeBetweenScraping = 2000;

        public static HtmlDocument GetDocumentOrNullIfError(string url)
        {
            var currentTime = DateTime.Now;

            // Ensure every new website call is at least minimumTimeBetweenScraping milliseconds apart, so as to not
            // burden the site we're scraping
            if (timeOfLastScrape.AddMilliseconds(minimumTimeBetweenScraping) < currentTime)
            {
                if (timeOfLastScrape != DateTime.MinValue)
                {
                    var deltaTime = currentTime - timeOfLastScrape.AddMilliseconds(minimumTimeBetweenScraping);
                    System.Threading.Thread.Sleep(deltaTime);
                }
            }
            var website = new HtmlWeb();
            HtmlDocument doc;

            try
            {
                doc = website.Load(url);
                website.PostResponse = (request, response) =>
                {
                    if (response == null)
                    {
                        WriteErrorMessage(url);
                        doc = null;
                    }
                };
            }
            catch (System.Net.WebException)
            {
                WriteErrorMessage(url);
                doc = null;
            }

            timeOfLastScrape = DateTime.Now;
            return doc;
        }

        private static void WriteErrorMessage(string url)
        {
            System.Console.WriteLine($"The webpage at {url} could not be reached.");
        }
    }
}
