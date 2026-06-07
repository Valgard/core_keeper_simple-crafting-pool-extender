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

1. **`SimpleCraftingUIContainer.ShowCraftingUI` (Prefix)** — clones the last
   `SimpleCraftingUI` on demand whenever more windows are needed, up to a
   configurable maximum (default 5).
2. **`CraftingCategoryNavigationUI.LateUpdate` (Postfix)** — the green
   up/down nav widget's x-position is hardcoded only for counts 1/2/3.
   The patch extrapolates the formula (`−2.5 × count − 0.3125`) for counts
   ≥ 4 so the widget sits correctly to the left of the rendered windows.

The growth is on-demand (no work until a workbench is opened) and idempotent
(repeat opens cost nothing). Counts 1–3 stay at the exact vanilla positions.

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

The mod is **side-effect-free** for vanilla and other mods: counts 1–3 are
untouched, growth is bounded at `maxPoolSize`, and the pool entries are real
Unity-instantiated `SimpleCraftingUI` clones (so styles, masks, layout all
auto-work the same way the vanilla 3 do).

## Configuration

There is no runtime `config.json` — Pugstorm's RoslynCSharp sandbox blocks
file I/O. The single tunable is a source-level constant in
`unity/SimpleCraftingPoolExtender/ModConfig.cs`:

| Field | Default | Effect |
|---|---|---|
| `maxPoolSize` | `5` | Hard ceiling on the pool. Safety net against a runaway mod growing the pool indefinitely. 5 covers a vanilla workbench (18 slots) plus a typical mod-added item (1–7 extra slots → 4–5 windows total). |

## License

Personal-use, non-commercial — Pugstorm Core Keeper EULA. Built against the
official `CoreKeeperModSDK`. Source on GitHub; contributions welcome.
