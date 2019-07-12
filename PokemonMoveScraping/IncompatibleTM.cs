using HtmlAgilityPack;
using Fraser.GenericMethods;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PokemonMoveScraping
{
    class IncompatibleTM
    {
        public static Dictionary<string, HashSet<string>> GetDictOfGeneralTMsAndIncompatiblePokemon()
        {
            var setOfPokemonThatCannotLearnGeneralTMs = new HashSet<string>();
            var dictOfTMsAndPokemonExceptions = new Dictionary<string, HashSet<string>>();

            var tmDoc = HtmlDocumentHandler.GetDocumentOrNullIfError("https://bulbapedia.bulbagarden.net/wiki/TM");

            // The h2 tag with the span of id "Incompatible_Pok..." is the "Incompatible Pokémon" section of the wiki
            // page
            // Select the first nested table within this section
            var tableOfPokemonThatCannotLearnGeneralTMs = tmDoc.DocumentNode.SelectSingleNode("//h2[span" +
                "[starts-with(@id, 'Incompatible_Pok')]]/following-sibling::table//table");

            FillSetOfPokemonAndDictOfExceptions(dictOfTMsAndPokemonExceptions, setOfPokemonThatCannotLearnGeneralTMs,
                tableOfPokemonThatCannotLearnGeneralTMs);

            var tableOfGeneralTMs = tmDoc.DocumentNode.SelectSingleNode("//h2[span" +
                "[@id='Near-universal_TMs']]/following-sibling::table//table");

            var dictOfGeneralTMsAndIncompatiblePokemon = new Dictionary<string, HashSet<string>>();
            FillDictOfGeneralTMsAndIncompatiblePokemon(dictOfGeneralTMsAndIncompatiblePokemon,
                setOfPokemonThatCannotLearnGeneralTMs, tableOfGeneralTMs);

            foreach (var move in dictOfTMsAndPokemonExceptions.Keys)
            {
                if (!dictOfGeneralTMsAndIncompatiblePokemon.ContainsKey(move))
                {
                    continue;
                }
                dictOfGeneralTMsAndIncompatiblePokemon[move] = dictOfGeneralTMsAndIncompatiblePokemon[move].Except(
                    dictOfTMsAndPokemonExceptions[move]).ToHashSet();
            }

            return dictOfGeneralTMsAndIncompatiblePokemon;
        }

        static void FillSetOfPokemonAndDictOfExceptions(Dictionary<string, HashSet<string>> dictOfTMsAndPokemonExceptions,
            HashSet<string> setOfPokemonThatCannotLearnGeneralTMs,
            HtmlNode tableOfPokemonThatCannotLearnGeneralTMs)
        {
            var dataRowsInTable = tableOfPokemonThatCannotLearnGeneralTMs.SelectNodes(".//tr[td]");

            foreach (var dataRow in dataRowsInTable)
            {
                var pokemonName = dataRow.SelectSingleNode("./td[3]").InnerText.Trim();

                // Each data row represents a Pokémon that cannot learn general TMs
                // The last cell and penultimate cell in the row are the exceptions in different generations of the
                // Pokémon games (The general TM moves the current Pokémon can learn)
                var compatibleTMLinks = dataRow.SelectNodes("./td[position()>last()-2]/a");

                setOfPokemonThatCannotLearnGeneralTMs.Add(pokemonName);
                if (compatibleTMLinks is null)
                {
                    continue;
                }

                foreach (var link in compatibleTMLinks)
                {
                    var moveName = link.InnerText.Trim();
                    AddDefaultSetValueToDict(dictOfTMsAndPokemonExceptions, moveName, pokemonName);
                }
            }
        }

        static void FillDictOfGeneralTMsAndIncompatiblePokemon(
            Dictionary<string, HashSet<string>> dictOfGeneralTMsAndIncompatiblePokemon,
            HashSet<string> setOfPokemonThatCannotLearnGeneralTMs,
            HtmlNode tableOfGeneralTMs)
        {
            var dataRowsInTable = tableOfGeneralTMs.SelectNodes(".//tr[td]");

            foreach (var dataRow in dataRowsInTable)
            {
                var nameOfTM = dataRow.SelectSingleNode("./td[1]").InnerText.Trim();

                // Each data row represents a general TM
                // The last cell in each row has a list of Pokémon that do not belong to the set of Pokémon that cannot
                // learn general TMs, but still cannot learn the current TM
                var additionalIncompatiblePokemon = dataRow.SelectSingleNode("./td[last()]");
                var additionalIncompatiblePokemonInnerText = additionalIncompatiblePokemon.InnerText.Trim();

                // If the inner text is the word "None", then there are no additional Pokémon that cannot learn the
                // current TM
                // Thus, the only Pokémon that cannot learn the current TM belong to the set of Pokémon that cannot
                // learn general TMs

                if (additionalIncompatiblePokemonInnerText == "None")
                {
                    dictOfGeneralTMsAndIncompatiblePokemon[nameOfTM] = setOfPokemonThatCannotLearnGeneralTMs;
                }
                else
                {
                    var allPokemonThatCannotLearnTM = GetAllPokemonThatCannotLearnTM(setOfPokemonThatCannotLearnGeneralTMs,
                        additionalIncompatiblePokemon);
                    dictOfGeneralTMsAndIncompatiblePokemon[nameOfTM] = allPokemonThatCannotLearnTM;
                }
            }
        }

        static HashSet<string> GetAllPokemonThatCannotLearnTM(HashSet<string> setOfPokemonThatCannotLearnGeneralTMs,
            HtmlNode additionalIncompatiblePokemonNode)
        {
            // Any specific Pokémon that cannot learn the current TM is represented by its image, which links
            // to that Pokémon's wiki page
            // If the image is followed by the text "(only in [specific game])", we ignore that Pokémon, since
            // this means there are some games in which they can learn the current TM
            var pokemonImages = additionalIncompatiblePokemonNode.SelectNodes(".//img" +
                // images which do not have an anchor-tag parent...
                "[not(parent::a" +
                    // that immediately preceeds text...
                    "[following-sibling::node()[1]" +
                        // containing the phrase "only in"
                        "[contains(., 'only in')]" +
                        "]" +
                    ")" +
                "]");

            var setOfAllIncompatiblePokemon = new HashSet<string>(setOfPokemonThatCannotLearnGeneralTMs);

            // If the inner text contains the phrase "and all genderless [Pokémon]", then we add the set of all
            // genderless Pokémon to the set of Pokémon unable to learn that TM
            if (additionalIncompatiblePokemonNode.InnerText.Contains("and all genderless"))
            {
                setOfAllIncompatiblePokemon = GetSetOfIncompatiblePokemonForGenderedTMs(setOfAllIncompatiblePokemon,
                    additionalIncompatiblePokemonNode);
            }

            if (pokemonImages is null)
            {
                return setOfAllIncompatiblePokemon;
            }

            foreach (var image in pokemonImages)
            {
                // Each specific Pokémon that cannot learn the current TM has its name stored as the alt text of its
                // image in this cell
                var altText = image.GetAttributeValue("alt", "").Trim();
                setOfAllIncompatiblePokemon.Add(altText);
            }
            return setOfAllIncompatiblePokemon;
        }

        static HashSet<string> GetSetOfIncompatiblePokemonForGenderedTMs(HashSet<string> setOfAllIncompatiblePokemon,
            HtmlNode additionalIncompatiblePokemonNode)
        {
            var setOfGenderlessPokemon = GetSetOfGenderlessPokemon();
            setOfAllIncompatiblePokemon.UnionWith(setOfGenderlessPokemon);

            // If the "and all genderless [Pokémon]" phrase is followed by "except [Pokémon X], [Pokémon Y], ...",
            // we remove these exceptions from our set of incompatible Pokémon
            if (additionalIncompatiblePokemonNode.InnerText.Contains("except"))
            {
                // All of these explicit exceptions are stored as the inner text of anchor tags, following the word
                // "except" in the table cell
                var genderlessPokemonExceptionNodes = additionalIncompatiblePokemonNode.SelectNodes(".//text()" +
                    "[contains(., 'except')]//following-sibling::a");
                foreach (var genderlessPokemon in genderlessPokemonExceptionNodes)
                {
                    setOfAllIncompatiblePokemon.Remove(genderlessPokemon.InnerText.Trim());
                }
            }
            return setOfAllIncompatiblePokemon;
        }

        static HashSet<string> GetSetOfGenderlessPokemon()
        {
            var setOfGenderlessPokemon = new HashSet<string>();
            var genderlessDoc = HtmlDocumentHandler.GetDocumentOrNullIfError("https://bulbapedia.bulbagarden.net/wiki/" +
                "Gender_unknown_(Egg_Group)");

            // The h2 tag with the span containing the text "Pokémon" is the "Pokémon" section of the wiki page
            // Select all nested tables within/after this section that have a header containing the text "Pokémon" in
            // its first row
            var tablesOfGenderlessPokemon = genderlessDoc.DocumentNode.SelectNodes("//h2[span" +
                "[text()='Pokémon']]/following-sibling::table//table[tr[1]/th[contains(., 'Pokémon')]]");

            foreach (var table in tablesOfGenderlessPokemon)
            {
                var pokemonInTable = table.SelectNodes(".//tr/td[3]");
                foreach (var pokemon in pokemonInTable)
                {
                    setOfGenderlessPokemon.Add(pokemon.InnerText.Trim());
                }
            }
            return setOfGenderlessPokemon;
        }

        static void AddDefaultSetValueToDict<T, V>(Dictionary<T, HashSet<V>> dictionary, T key, V value)
        {
            if (dictionary.ContainsKey(key))
            {
                dictionary[key].Add(value);
            }
            else
            {
                dictionary[key] = new HashSet<V> { value };
            }
        }
    }
}
