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

- **Pool growth.** Harmony Postfix on `SimpleCraftingUIContainer.Awake`.
  Clones the last existing `SimpleCraftingUI` and appends it until the pool
  reaches `maxPoolSize` (5 by default). Runs once per container, immediately
  after Vanilla's `Init()` loop and before anything is rendered — so the
  clone inherits the Inspector-default "all slots inactive" state.
- **Nav-UI position fix for counts ≥ 4.** Harmony Postfix on
  `CraftingCategoryNavigationUI.LateUpdate`. The vanilla switch only covers
  1/2/3 windows; the patch extrapolates the formula
  `−2.5 × count − 0.3125` so the up/down arrows + workbench-icon widget
  sit correctly to the left of the rendered windows for counts ≥ 4.
- **Configurable maximum** via `unity/SimpleCraftingPoolExtender/ModConfig.cs`:
  `maxPoolSize = 5`. Singleton API shape preserved so a future safe-IO
  config-loader can drop in without touching the patches.
