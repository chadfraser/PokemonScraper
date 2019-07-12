using Fraser.GenericMethods;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Net;

namespace PokemonMoveScraping
{
    class Program
    {
        static bool distinguishSubtypes = true;

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
                var moveName = moveNode.InnerText;
                totalMoveCount += GetCountOfPokemonToLearnMove(movePageUrl, moveName);
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
                var moveName = moveNode.InnerText;
                var pokemonMoveSet = GetSetOfPokemonToLearnMove(movePageUrl, moveName);

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

        static void GetDistinctPokemonFromTable(HtmlNode tableNode, HashSet<string> setOfPokemonToLearnMove, string moveName)
        {
            var nodesOfPokemonToLearnMove = tableNode.SelectNodes("./tr/td[3]");
            //Console.WriteLine(nodesOfPokemonToLearnMove.Count + " " + moveName);
            if (nodesOfPokemonToLearnMove is null)
            {
                var tableContentText = tableNode.SelectSingleNode(".//tr[3]").InnerText.Trim();
                tableContentText = WebUtility.HtmlDecode(tableContentText);

                Console.Error.WriteLine($"A learnset table for the move '{moveName}' had no pokemon stored in it.");
                Console.Error.WriteLine($"\t'{tableContentText}'");
                Console.Error.WriteLine();
                return;
            }

            foreach (var pokemonNode in nodesOfPokemonToLearnMove)
            {
                //Console.WriteLine(">>>" + pokemonNode.InnerText);
                ////Console.WriteLine("\n\n\n");
                var pokemonName = pokemonNode.SelectSingleNode(".//a").InnerText;
                //Console.WriteLine("<<<" + pokemonName);
                var pokemonSmallTextNode = pokemonNode.SelectSingleNode(".//small").InnerText;

                if (!string.IsNullOrEmpty(pokemonSmallTextNode) && distinguishSubtypes)
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

        static HashSet<string> GetSetOfPokemonToLearnMove(string movePageUrl, string moveName)
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
                "following-sibling::h2[1]/preceding-sibling::table[@class='roundy'][tr[1]/th[contains(., 'Pokémon')]]");

            var setOfPokemonToLearnMove = new HashSet<string>();

            if (tablesOfPokemonToLearnMove is null)
            {
                Console.WriteLine($"No learnset tables were found for the move '{moveName}'.");
            }
            else
            {
                //Console.WriteLine(tablesOfPokemonToLearnMove.Count);
                foreach (var tableNode in tablesOfPokemonToLearnMove)
                {
                    GetDistinctPokemonFromTable(tableNode, setOfPokemonToLearnMove, moveName);
                }
            }
            //catch (NullReferenceException)
            //{
            //    Console.Error.WriteLine("Move data could not be found for the move at the following page:");
            //    Console.Error.WriteLine($"\t{movePageUrl}");
            //    Console.Error.WriteLine("If the move does not normally have any set pokemon that can learn it (Such " +
            //        "as Struggle or Breakneck Blitz) or is a Z-move (such as Catastropika), this is not an error, as " +
            //        "those moves are intentionally ignored. Otherwise, you may wish to take note of this.");
            //    Console.Error.WriteLine();
            //    Console.Error.WriteLine();
            //}

            return setOfPokemonToLearnMove;
        }

        static int GetCountOfPokemonToLearnMove(string movePageUrl, string moveName)
        {
            var setOfPokemonToLearnMove = GetSetOfPokemonToLearnMove(movePageUrl, moveName);
            return setOfPokemonToLearnMove.Count;
        }
    }
}
