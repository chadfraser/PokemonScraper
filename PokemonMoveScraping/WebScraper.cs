using HtmlAgilityPack;
using Fraser.GenericMethods;
using System;

namespace PokemonMoveScraping
{
    class Program
    {
        static void Main(string[] args)
        {
            var mainSite = "https://bulbapedia.bulbagarden.net/wiki/List_of_moves";
            var doc = HtmlDocumentHandler.GetDocumentOrNullIfError(mainSite);
            Console.WriteLine("aaa");
            var movesTable = doc.DocumentNode.SelectSingleNode("//table//tr//td");
            var nodeListOfMoveNamesAndLinks = movesTable.SelectNodes(".//tr//td[2]//a");
            foreach (var moveNode in nodeListOfMoveNamesAndLinks)
            {
                var moveHrefAttribute = moveNode.Attributes["href"];
                if (moveHrefAttribute is null)
                {
                    continue;
                }
                var movePageUrlSuffix = moveHrefAttribute.Value;
                var movePageUrl = $"https://bulbapedia.bulbagarden.net{movePageUrlSuffix}";


            }
            Console.ReadLine();
            //aaa = doc.DocumentNode.SelectNodes("//table");
            //Console.WriteLine(aaa);
            //Console.WriteLine("   -------     ");
            //aaa = doc.DocumentNode.SelectNodes("//table/tbody");
            //Console.WriteLine(aaa);
            //Console.WriteLine("   -------     ");
            //aaa = doc.DocumentNode.SelectNodes("//table/tbody/tr");
            //Console.WriteLine(aaa);
            //Console.WriteLine("   -------     ");
            //aaa = doc.DocumentNode.SelectNodes("//table/tbody/tr/td[2]");
            //Console.WriteLine(aaa);
                //GetElementbyId("List_of_moves");
        }
    }
}
