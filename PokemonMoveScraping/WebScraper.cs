using Fraser.GenericMethods;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;

namespace PokemonMoveScraping
{
    class Program
    {
        static void Main(string[] args)
        {
            var mainSite = "https://bulbapedia.bulbagarden.net/wiki/List_of_moves";
            var mainDoc = HtmlDocumentHandler.GetDocumentOrNullIfError(mainSite);
            var movesTable = mainDoc.DocumentNode.SelectSingleNode("//table//tr//td");
            var nodeListOfMoveNamesAndLinks = movesTable.SelectNodes(".//tr//td[2]//a");

            var totalMoveCount = 0;

            foreach (var moveNode in nodeListOfMoveNamesAndLinks)
            {
                var moveHrefAttribute = moveNode.Attributes["href"];
                if (moveHrefAttribute is null)
                {
                    continue;
                }
                var movePageUrlSuffix = moveHrefAttribute.Value;
                var movePageUrl = $"https://bulbapedia.bulbagarden.net{movePageUrlSuffix}";

                var movePageDoc = HtmlDocumentHandler.GetDocumentOrNullIfError(movePageUrl);
                totalMoveCount += getCountOfPokemonToLearnMove(movePageDoc);
            }

            Console.WriteLine(totalMoveCount);
            Console.ReadLine();
        }

        static int getCountOfPokemonToLearnMove(HtmlDocument movePageDoc)
        {
            /*
             * Select all of the sibling tables between the h2 tag with the span of id "Learnset" (i.e., the Learnset
             * section), but before the next h2 tag (i.e., whatever the next section is).
             * This ensures we only get tables in the Learnset section of the wiki page, which is where the only relevant
             * tables are.
             * This may included tables titled "by leveling up", "by HM", "by event", etc.
             */
            var learnTables = movePageDoc.DocumentNode.SelectNodes("//h2[span[@id='Learnset']]/" +
                "following-sibling::h2[1]/preceding-sibling::table/tr/td[3]");

            var setOfPokemonToLearnThisMove = new HashSet<string>();
            var countOfPokemonToLearnThisMove = 0;
            try
            {
                foreach (var pokemonNode in learnTables)
                {
                    if (setOfPokemonToLearnThisMove.Contains(pokemonNode.InnerText))
                    {
                        //Console.WriteLine(pokemonNode.InnerText);
                        continue;
                    }
                    setOfPokemonToLearnThisMove.Add(pokemonNode.InnerText);
                    countOfPokemonToLearnThisMove++;
                }
            }
            catch (NullReferenceException e)
            {
                Console.WriteLine(movePageDoc.DocumentNode.SelectSingleNode("/html/head/title").OuterHtml);
                Console.ReadKey();
            }
            return countOfPokemonToLearnThisMove;
        }
    }
}
