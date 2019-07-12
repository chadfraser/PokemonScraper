# PokemonScraper

A friend asked me a random question the other day that she could not find the answer to anywhere online.

"If they were to give every pokemon a unique animation for every move it could use, how many animations would they have to make overall?"

I didn't know the answer, but figured it wouldn't be too hard to find - Just build a program to scrape the pokemon wiki page for a list of every move in the game, then go to the wiki page for each of those moves and scrape a list of all pokemon that can use that move.

Of course, it wasn't quite that easy. Some moves are pretty exceptional, like <b>Struggle</b> (No one pokemon can 'learn' it, but every pokemon automatically uses it if there are no other moves they can use).
There are also Z-moves, like <b>Breakneck Blitz</b> that don't have any specific pokemon that can learn them, but instead can be performed by:

> Any non-Mega Evolved, non-Primal Pokémon [who] knows a damaging Normal-type move, holds a Normalium Z, and if its Trainer wears a Z-Ring or Z-Power Ring.

Also, there are some moves taught by TM items that, for brevity's sake, the wiki page doesn't list which pokemon can learn those moves. Instead, it lists which pokemon <i>cannot</i> learn those moves.

And finally, there's the question on what counts as a 'different pokemon'. For example, the <b>Vulpix</b> pokemon can learn certain moves, but the <b>Alolan Vulpix</b> subtype learns completely different moves. On the other hand, the <b>Partner Pikachu</b> subtype can learn every move a normal <b>Pikachu</b> can, plus four additional moves that the normal form cannot learn. Should they be classified differently, or both under the umbrella of 'Moves a Pikachu can learn'?

Evidently, this question requires some finesse to even ask, let alone solve.
