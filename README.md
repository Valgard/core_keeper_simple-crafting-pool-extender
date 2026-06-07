# Simple Crafting Pool Extender

A Core Keeper helper mod for **other mods**. It lets mods add a **4th tab and
beyond** to vanilla Simple-Crafting workbenches (Iron Workbench, Wood
Workbench, Copper Workbench, Carpenter, Forge, etc.) so their items show up
in the crafting UI instead of being silently dropped.

If you are a **player**, you only need this mod if another mod you are
subscribed to lists it as a dependency. By itself it does nothing visible.

## What it fixes

Vanilla `SimpleCraftingUIContainer` ships with a hardcoded list of 3
`SimpleCraftingUI` entries — enough for a workbench's three 6-slot tabs (Gear,
Crafting, Base). When a mod injects a new craftable item into a vanilla
workbench, a 4th tab is needed; vanilla logs

```
Not enough SimpleCraftingUIs in SimpleCraftingUIContainer to show all recipes.
Needed at least 4, but only have 3.
```

…and the mod's item never appears.

This mod patches two methods:

1. **`SimpleCraftingUIContainer.Awake` (Postfix)** — clones the last
   `SimpleCraftingUI` and appends it until the pool reaches a configurable
   maximum (default 5). Runs once per container, immediately after Vanilla's
   `Init()` loop and before anything is rendered, so the clone inherits the
   Inspector-default "all slots inactive" state.
2. **`CraftingCategoryNavigationUI.LateUpdate` (Postfix)** — the green
   up/down nav widget's x-position is hardcoded only for counts 1/2/3.
   The patch extrapolates the formula (`−2.5 × count − 0.3125`) for counts
   ≥ 4 so the widget sits correctly to the left of the rendered windows.

## Requirements

- Core Keeper (verified on 1.2.1.4)
- No CoreLib dependency — this mod runs standalone

## Installation (players)

Subscribe in-game through the **Mods** menu (or on the mod.io website) and
restart. You only need this if another mod requires it; the listing in your
mod manager will say so.

## For mod authors

If you add an item to a vanilla workbench via `canCraftObjects.Add(...)` (or
the equivalent `InjectCraftableObject` pattern), list this mod as a
dependency in your `ModBuilderSettings.asset`:

```yaml
dependencies:
  - modName: SimpleCraftingPoolExtender
    required: 1
```

mod.io will then prompt players to install both. Without this dependency,
your item's recipe will register but never render in the workbench UI on
players who don't already have a pool-expanding mod.

## Side-effect-free for vanilla

- Counts 1–3 stay at the exact vanilla positions (Postfix early-outs).
- Unused pool entries beyond `amountOfWindowsShowing` are hidden by Vanilla's
  own `HideCraftingUI()` loop — zero render cost.
- The clone is a real `UnityEngine.Object.Instantiate` of the last existing
  entry — styles, masks, and layout inherit cleanly, and the visual state
  starts in the Inspector-default "all slots inactive" condition because
  the clone happens before any workbench has rendered.

## License

Personal-use, non-commercial — Pugstorm Core Keeper EULA. Built against the
official `CoreKeeperModSDK`. Source on GitHub; contributions welcome.
