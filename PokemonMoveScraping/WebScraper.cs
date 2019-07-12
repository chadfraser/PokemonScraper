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
                totalMoveCount += getCountOfPokemonToLearnMove(movePageUrl);

                //var movePageDoc = HtmlDocumentHandler.GetDocumentOrNullIfError(movePageUrl);
                //totalMoveCount += getCountOfPokemonToLearnMove(movePageDoc);
            }

            Console.WriteLine(totalMoveCount);
            Console.ReadLine();
        }

        static int getCountOfPokemonToLearnMove(String movePageUrl)
        {
            var movePageDoc = HtmlDocumentHandler.GetDocumentOrNullIfError(movePageUrl);
            /*
             * Select all of the sibling tables between the h2 tag with the span of id "Learnset" (i.e., the Learnset
             * section), but before the next h2 tag (i.e., whatever the next section is).
             * This ensures we only get tables in the Learnset section of the wiki page, which is where the only relevant
             * tables are.
             * This may included tables titled "by leveling up", "by HM", "by event", etc.
             */
            var tablesOfPokemonToLearnMove = movePageDoc.DocumentNode.SelectNodes("//h2[span[@id='Learnset']]/" +
                "following-sibling::h2[1]/preceding-sibling::table/tr/td[3]");

            var setOfPokemonToLearnThisMove = new HashSet<string>();
            try
            {
                foreach (var pokemonNode in tablesOfPokemonToLearnMove)
                {
                    // It is possible one pokemon could be on multiple tables at once for the same move (e.g., if the
                    // pokemon can learn the move by leveling up or by HM).
                    if (setOfPokemonToLearnThisMove.Contains(pokemonNode.InnerText))
                    {
                        continue;
                    }
                    setOfPokemonToLearnThisMove.Add(pokemonNode.InnerText);
                }
            }
            catch (NullReferenceException ignore)
            {
                Console.Error.WriteLine("Move data could not be found for the move at the following page:");
                Console.Error.WriteLine($"\t{movePageUrl}");
                Console.Error.WriteLine("If the move does not normally have any set pokemon that can learn it (Such " +
                    "as Struggle or Breakneck Blitz) or is a Z-move (such as Catastropika), this is not an error, as " +
                    "those moves are intentionally ignored. Otherwise, you may wish to take note of this.");
                Console.Error.WriteLine();
                Console.Error.WriteLine();
                Console.ReadKey();
            }
            return setOfPokemonToLearnThisMove.Count;
        }
    }
}
