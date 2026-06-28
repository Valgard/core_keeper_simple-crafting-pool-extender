# Simple Crafting Pool Extender

**A small helper mod for other mods.**

It lets mods add a 4th crafting tab (and beyond) to vanilla Simple-Crafting
workbenches — starting with the Iron Workbench — so their items actually appear
in the crafting UI instead of being silently dropped.

## Do I need this?

Only if another mod you use lists it as a dependency. On its own it adds nothing
you can see — it's plumbing that other mods rely on. If a mod requires it, your
mod manager and mod.io will prompt you to install both.

## What it fixes

Vanilla workbenches have room for exactly 3 crafting tabs. When a mod adds a
craftable that needs a 4th, the game runs out of tab slots and the recipe never
shows up (it logs "Not enough SimpleCraftingUIs… Needed at least 4, but only
have 3"). This mod raises the limit to 5 tabs and fixes the navigation-arrow
position so the extra tabs page correctly. It applies to the Iron, Wood and
Copper Workbenches, Carpenter's Table, Forge, and other Simple-Crafting
stations.

## Safe for vanilla

- Workbenches using only 1–3 tabs are completely unchanged.
- Unused tab slots cost nothing — the game hides them exactly as it always has.

## Requirements

- Core Keeper (verified on 1.2.1.4)

## For mod authors

Adding an item to a vanilla workbench? List SimpleCraftingPoolExtender as a
required dependency in your ModBuilderSettings and mod.io will prompt players to
install both. Without it, your recipe registers but never renders for players
who don't already have a pool-expanding mod.

---

*Built with the official Pugstorm Core Keeper Mod SDK. Personal-use,
non-commercial (Core Keeper EULA). Not affiliated with or endorsed by
Pugstorm.*
