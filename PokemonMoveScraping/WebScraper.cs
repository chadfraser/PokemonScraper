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
                //var moveHrefAttribute = moveNode.Attributes["href"];
                //if (moveHrefAttribute is null)
                //{
                //    continue;
                //}
                //var movePageUrlSuffix = moveHrefAttribute.Value;
                //var movePageUrl = $"https://bulbapedia.bulbagarden.net{movePageUrlSuffix}";
                //var movePageUrl = "https://bulbapedia.bulbagarden.net/wiki/Water_Gun_(move)";
                var movePageUrl = "https://bulbapedia.bulbagarden.net/wiki/Pound_(move)";

                var newPageDoc = HtmlDocumentHandler.GetDocumentOrNullIfError(movePageUrl);
                var learnTables = newPageDoc.DocumentNode.SelectNodes("//h2[span[@id='Learnset']]/" +
                    "following-sibling::h2[1]/preceding-sibling::table/tr/td[3]");
                //var learnTables = newPageDoc.DocumentNode.SelectNodes("//h2[span[@id='Learnset']]/" +
                //  "following-sibling::table/tr/td[3]");
                foreach (var pokemonNode in learnTables)
                {
                    Console.WriteLine(pokemonNode.InnerText);
                    Console.ReadLine();
                }
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
