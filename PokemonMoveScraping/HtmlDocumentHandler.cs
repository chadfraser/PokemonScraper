using HtmlAgilityPack;

namespace Fraser.GenericMethods
{
    public class HtmlDocumentHandler
    {
        public static HtmlDocument GetDocumentOrNullIfError(string url)
        {
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
            System.Threading.Thread.Sleep(500);
            return doc;
        }

        public static void WriteErrorMessage(string url)
        {
            System.Console.WriteLine($"The webpage at {url} could not be reached.");
        }
    }
}
