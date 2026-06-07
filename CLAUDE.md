# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this repo is

A Core Keeper **helper mod** for other mods. Vanilla
`SimpleCraftingUIContainer.simpleCraftingUIs` is an editor-hardcoded
list of 3 entries — enough for a workbench's three 6-slot tabs. When a
mod injects a new craftable into a vanilla workbench and pushes the
recipe range past 18 slots, vanilla logs *"Not enough SimpleCraftingUIs
… Needed at least 4, but only have 3"* and the new item never appears.
This mod grows that pool to `maxPoolSize` (5 by default) so a 4th and
5th tab can render. Two Harmony patches against Pugstorm's
`CoreKeeperModSDK`. No content of its own. Personal-use, non-commercial
(Pugstorm EULA).

It is intended to be declared as a **required dependency** by mods that
add craftable items to vanilla workbenches (the sibling
`caveling-divining-rod` does exactly this in its
`unity/CavelingDiviningRod.asset`).

The parent `../CLAUDE.md` holds the mod-agnostic SDK/CrossOver guidance
shared with the sibling mods.

## Build and deploy

```bash
source .envrc           # exports UNITY_BIN, SDK_PATH, MOD_INSTALL_PATH, MOD_NAME, …
../utils/build.sh       # Unity batchmode build; on Darwin auto-runs install-macos.sh
```

Unity Editor must be closed (it locks the project). `utils/link.sh`
symlinks the repo's `unity/` mirror into `$SDK_PATH/Assets/`: one
**directory** symlink for `unity/SimpleCraftingPoolExtender/`, plus
three file symlinks for the Assets-level files beside it
(`SimpleCraftingPoolExtender.asset`, `.asset.meta`, `.meta`).
`build.sh` invokes it idempotently on every run, so worktree switches
and repo moves self-heal.

No automated tests — verification is a manual in-game check. The most
realistic scenario is using the sibling `caveling-divining-rod`
(or any other mod that adds a craftable to Iron Workbench): open the
Iron Workbench, confirm a 4th tab appears with the mod-added item.
A synthetic 5-window test (no mod-items needed) lived briefly during
0.9.0 development as `TestForceWindowCountPatch.cs` + a
`testForceWindowCount` field on ModConfig — both were removed before
release; re-introduce if needed for layout work on counts > 4.

## Architecture

Three patch classes plus bootstrap + config in the
`SimpleCraftingPoolExtender` namespace, plus shared editor helpers
symlinked in from `../utils/`:

- **`SimpleCraftingPoolExtenderMod` (`IMod`)** — bootstrap. EarlyInit
  logs the configured `maxPoolSize`. No `BurstDisabler` needed: neither
  patch target is Burst-compiled. No CoreLib usage — this mod runs
  standalone.
- **`ModConfig`** — hardcoded singleton with `maxPoolSize = 5`.
  RoslynCSharp sandbox blocks `System.IO` so no runtime config file.
  The singleton shape (`ModConfig.Instance.maxPoolSize`) is kept so a
  future safe-IO config-loader could drop in without touching the
  patches.
- **`SimpleCraftingPoolGrowthPatch`** — `Postfix` on
  `SimpleCraftingUIContainer.Awake`. Runs **once** per container
  spawn, immediately after vanilla's `Init()` loop over the 3
  original entries and before anything is ever rendered. Clones the
  last existing `SimpleCraftingUI` via
  `UnityEngine.Object.Instantiate(template.gameObject, parent)` and
  appends to `simpleCraftingUIs` until the list reaches `maxPoolSize`.
  Idempotent (no-op if already at target). See "Why Awake-Postfix"
  below for the trade-off vs the originally-tried
  ShowCraftingUI-Prefix.
- **`CraftingNavUIPositionPatch`** — `Postfix` on
  `CraftingCategoryNavigationUI.LateUpdate`. Vanilla's `switch`
  hardcodes x-positions only for `amountOfWindowsShowing = 1/2/3`
  (`−2.8125`, `−5.3125`, `−7.8125`); at counts ≥ 4 the switch falls
  through silently and the nav widget sticks at whatever position it
  had last frame. The patch extrapolates the verified formula
  `−2.5 × count − 0.3125` for counts ≥ 4. Early-outs for 1-3 so
  vanilla behaviour is preserved exactly.
- **Shared editor helpers** (`../utils/CLIBuildHelper.cs`,
  `CLIPublishHelper.cs`, `LocalizationGenerator.cs`, namespace
  `CoreKeeperModUtils`) — symlinked into
  `unity/SimpleCraftingPoolExtender/Editor/` by `utils/link.sh`.
  `LocalizationGenerator` is a no-op here — this mod ships no
  `localization.yaml`. The `.cs` symlinks and their Unity-generated
  `.meta` are gitignored.

`unity/` is the canonical source — a 1:1 mirror of the SDK's `Assets/`
tree holding every file the Unity Editor generates for the mod: the
`.cs` sources, both `.asmdef` files, the ModBuilderSettings `.asset`,
and all `.meta` files (GUID carriers — versioned per Unity convention).
The SDK clone's `Assets/SimpleCraftingPoolExtender` is a **directory
symlink** into `unity/SimpleCraftingPoolExtender/` so any file the
Editor adds is captured automatically.

The runtime `SimpleCraftingPoolExtender.asmdef` is the SDK wizard's
output unmodified — the game-DLL reference set covers everything our
patches need (`Pug.Other.dll` for `SimpleCraftingUIContainer` /
`CraftingCategoryNavigationUI`, `0Harmony.dll` for the Harmony
attributes, `PugMod.SDK.Runtime.dll` for `IMod`).

Patch targets were identified by decompiling the SDK's bundled game
DLLs (`Pug.Other.dll`) with `ilspycmd`.

## Why Awake-Postfix (not ShowCraftingUI-Prefix)

The 0.9.0 development line first tried a `[HarmonyPrefix]` on
`SimpleCraftingUIContainer.ShowCraftingUI` — "grow on demand": compute
how many windows the active category needs, clone the last entry up
to that count, then let vanilla run. Idempotent on subsequent opens.
Conceptually cleaner than always pre-allocating.

It broke visually. On a workbench whose 4th tab carried a single
mod-added recipe, the 4th tab rendered the real item **plus** the
template's earlier-rendered items underneath. The cause:
`Object.Instantiate` deep-copies the template's full visual state,
including each `itemSlot[i].UpdateSlot()` sprite from whichever
workbench the template last rendered. Vanilla's
`RecipesUI.ShowContainerUI` activates only slots with an active
recipe and deactivates the rest — but inactive `SpriteRenderer`s
still carry their last sprite, which shows through.

Awake fires once, **before any workbench has rendered**. Cloning at
that point captures the template in its Inspector-default
"all slots inactive" state, no sprite bleed-through, no run-time
race with a prior render. Trade-off: every SimpleCrafting workbench
now has 5 pool entries even when only 1-3 are needed. This is free
for vanilla because `ShowCraftingUI` explicitly calls `HideCraftingUI()`
on every entry beyond `amountOfWindowsShowing` (= `root.SetActive(false)`,
zero render cost).

If you reopen the on-demand path in the future, the clone must be
forced into the Inspector-default visual state (slots inactive +
SpriteRenderer.sprite cleared) before vanilla activates it. The
Awake-Postfix path sidesteps the whole problem.

## macOS / CrossOver

The mod is deployed through the fake-mod.io workaround (see parent
`../CLAUDE.md`). This mod's fake mod.io ID is **`9999995`** (the
siblings `disable-durability` uses `9999999`, `faster-talents` uses
`9999998`, `item-checklist` uses `9999997`, `caveling-divining-rod` uses
`9999996` — they must differ).
Do not open the in-game Mods menu while a fake-ID install is active;
re-run `../utils/build.sh` to restore if the cache is wiped.

## Publishing to mod.io

`../utils/upload.sh` publishes this mod. It runs the shared Editor
class `CoreKeeperModUtils.CLIPublishHelper.Publish` (symlinked in from
`../utils/`, alongside `CLIBuildHelper`) via Unity batchmode. The
publish reads `MOD_REPO_ROOT` (set in `.envrc`) to locate
`CHANGELOG.md`.

- `Editor/SimpleCraftingPoolExtender.Editor.asmdef` references the
  mod.io plugin DLL via `overrideReferences: true` +
  `precompiledReferences: ["modio.UnityPlugin.dll"]`.
- The published version comes from the topmost `## [x.y.z]` entry of
  `CHANGELOG.md`; bump it before publishing.
- The profile logo is `unity/SimpleCraftingPoolExtender/Editor/logo.png`
  (readable, uncompressed; min 512×288).
- The real mod ID will live in
  `unity/SimpleCraftingPoolExtender/Editor/SimpleCraftingPoolExtender_modio.asset`
  after the first successful publish.
- One-time: log in via the SDK window's "Log in" tab before the first
  publish.

**Dependency ordering across the mod family:** this mod must be
published **first**, before any mod that lists it as a dependency.
Subscribers need to be able to install it through mod.io's dependency
resolution; if a dependent mod publishes first, the loader can't
resolve `SimpleCraftingPoolExtender` and the dependent fails to
install. Currently the only dependent in this repo family is
`caveling-divining-rod`.

## Conventions

- Commit messages: short imperative subject, no emoji, body wrapped ~75 chars.
- Documentation files (`CLAUDE.md`, `README.md`, `docs/`) are English; chat answers are German.
- The user prefers `git commit --amend` / `git reset --soft` over fix-up commits on a personal branch, and `git rebase` over `git merge`.
