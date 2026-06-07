# Changelog

All notable changes to this mod are documented in this file. The format is
loosely based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
without strict adherence. The topmost `## [x.y.z]` entry is the version
`upload.sh` publishes.

## [0.9.0] - 2026-06-07

First public release — a deliberate pre-1.0 version. The feature scope is
small and focused; the pre-1.0 framing leaves room for fast follow-ups
(per-workbench overrides, stricter validation) before committing to 1.0.

### Added

- **On-demand pool growth.** Harmony Prefix on
  `SimpleCraftingUIContainer.ShowCraftingUI`. When the active category needs
  more windows than the vanilla 3-entry pool can supply, the patch clones the
  last existing `SimpleCraftingUI` and appends it — up to `maxPoolSize` (5 by
  default). Idempotent on subsequent opens.
- **Nav-UI position fix for counts ≥ 4.** Harmony Postfix on
  `CraftingCategoryNavigationUI.LateUpdate`. The vanilla switch only covers
  1/2/3 windows; the patch extrapolates the formula
  `−2.5 × count − 0.3125` so the up/down arrows + workbench-icon widget
  sit correctly to the left of the rendered windows for counts ≥ 4.
- **Configurable maximum** via `unity/SimpleCraftingPoolExtender/ModConfig.cs`:
  `maxPoolSize = 5`. Singleton API shape preserved so a future safe-IO
  config-loader can drop in without touching the patches.

### Side-effect-free for vanilla

- Counts 1–3 stay at the exact vanilla positions (Postfix early-outs).
- No pool growth happens until at least one mod-added item exceeds the
  vanilla 18-slot limit.
- The clone is a real `UnityEngine.Object.Instantiate` of the last existing
  entry — styles, masks, and layout inherit cleanly.
