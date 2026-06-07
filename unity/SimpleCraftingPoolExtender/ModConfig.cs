namespace SimpleCraftingPoolExtender
{
    /// <summary>
    /// Mod configuration. Values are hardcoded constants: Pugstorm's
    /// RoslynCSharp sandbox blocks System.IO, so a runtime config.json
    /// cannot be read. The singleton shape
    /// (ModConfig.Instance.field) is kept so a future config loader could
    /// drop in without touching the patch classes.
    /// </summary>
    internal sealed class ModConfig
    {
        // Hard ceiling on the SimpleCraftingUIContainer pool. The growth
        // patch will not clone past this number — the safety net against a
        // runaway mod (or a freak combination of mods) that would otherwise
        // grow the pool indefinitely. 5 is enough for Iron Workbench + one
        // typical mod-added item (Vanilla = 3 windows × 6 = 18 slots; +1
        // mod item = 4 windows; +7 mod items = 5 windows).
        public int maxPoolSize = 5;

        private static readonly ModConfig _instance = new ModConfig();
        public static ModConfig Instance => _instance;
    }
}
