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
            var nodeListOfMoveNamesAndLinks = GetNodeListOfAllMoves();
            //var totalMoveCount = GetTotalCountOfLearnedMoves(nodeListOfMoveNamesAndLinks);

            //Console.WriteLine($"In the main series pokemon game, there are a total of {totalMoveCount} move outcomes " +
            //    $"among all pokemon.");
            //Console.WriteLine("Press enter to continue.");
            //Console.ReadLine();

            var pokemonMoveDict = GetDictOfAllPokemonAndTheirLearnedMoves(nodeListOfMoveNamesAndLinks);
            foreach (var pokemon in pokemonMoveDict.Keys)
            {
                var formattedMoveSet = string.Join(", ", pokemonMoveDict[pokemon]);
                Console.WriteLine($"{pokemon} can learn {pokemonMoveDict[pokemon].Count} moves: " +
                    $"{formattedMoveSet}.");
            }
            Console.WriteLine("Press enter to continue.");
            Console.ReadLine();
        }

        static HtmlNodeCollection GetNodeListOfAllMoves()
        {
            var mainSite = "https://bulbapedia.bulbagarden.net/wiki/List_of_moves";
            var mainDoc = HtmlDocumentHandler.GetDocumentOrNullIfError(mainSite);
            var movesTable = mainDoc.DocumentNode.SelectSingleNode("//table//tr//td");
            var nodeListOfMoveNamesAndLinks = movesTable.SelectNodes(".//tr//td[2]//a");
            return nodeListOfMoveNamesAndLinks;
        }

        static int GetTotalCountOfLearnedMoves(HtmlNodeCollection nodeListOfMoveNamesAndLinks)
        {
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
                totalMoveCount += GetCountOfPokemonToLearnMove(movePageUrl);
            }

            return totalMoveCount;
        }

        static Dictionary<string, HashSet<string>>
            GetDictOfAllPokemonAndTheirLearnedMoves(HtmlNodeCollection nodeListOfMoveNamesAndLinks)
        {
            var pokemonAndMoves = new Dictionary<string, HashSet<string>>();

            foreach (var moveNode in nodeListOfMoveNamesAndLinks)
            {
                var moveHrefAttribute = moveNode.Attributes["href"];
                if (moveHrefAttribute is null)
                {
                    continue;
                }
                var movePageUrlSuffix = moveHrefAttribute.Value;
                var movePageUrl = $"https://bulbapedia.bulbagarden.net{movePageUrlSuffix}";
                var pokemonMoveSet = GetSetOfPokemonToLearnMove(movePageUrl);
                var moveName = moveNode.InnerText;

                foreach (var pokemon in pokemonMoveSet)
                {
                    if (pokemonAndMoves.ContainsKey(pokemon))
                    {
                        pokemonAndMoves[pokemon].Add(moveName);
                    }
                    else
                    {
                        pokemonAndMoves[pokemon] = new HashSet<string> { moveName };
                    }
                }
            }

            return pokemonAndMoves;
        }

        static HashSet<string> GetSetOfPokemonToLearnMove(string movePageUrl)
        {
            movePageUrl = "https://bulbapedia.bulbagarden.net/wiki/Protect_(move)";
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

            var setOfPokemonToLearnMove = new HashSet<string>();
            try
            {
                foreach (var pokemonNode in tablesOfPokemonToLearnMove)
                {
                    Console.WriteLine(">>>" + pokemonNode.InnerText);
                    var pokemonName = pokemonNode.SelectSingleNode(".//a").InnerText;
                    Console.WriteLine("<<<" + pokemonName);
                    var pokemonSmallTextNode = pokemonNode.SelectSingleNode(".//small").InnerText;

                    if (!string.IsNullOrEmpty(pokemonSmallTextNode))
                    {
                        pokemonName = $"{pokemonName} ({pokemonSmallTextNode})";
                    }

                    // It is possible one pokemon could be on multiple tables at once for the same move (e.g., if the
                    // pokemon can learn the move by leveling up or by HM).
                    if (setOfPokemonToLearnMove.Contains(pokemonName))
                    {
                        continue;
                    }
                    setOfPokemonToLearnMove.Add(pokemonName);
                }
            }
            catch (NullReferenceException)
            {
                Console.Error.WriteLine("Move data could not be found for the move at the following page:");
                Console.Error.WriteLine($"\t{movePageUrl}");
                Console.Error.WriteLine("If the move does not normally have any set pokemon that can learn it (Such " +
                    "as Struggle or Breakneck Blitz) or is a Z-move (such as Catastropika), this is not an error, as " +
                    "those moves are intentionally ignored. Otherwise, you may wish to take note of this.");
                Console.Error.WriteLine();
                Console.Error.WriteLine();
            }

            return setOfPokemonToLearnMove;
        }

        static int GetCountOfPokemonToLearnMove(string movePageUrl)
        {
            var setOfPokemonToLearnMove = GetSetOfPokemonToLearnMove(movePageUrl);
            return setOfPokemonToLearnMove.Count;
        }
    }
}
