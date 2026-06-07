using PugMod;
using UnityEngine;

namespace SimpleCraftingPoolExtender
{
    /// <summary>
    /// Helper mod that lets other mods add a 4th+ tab to vanilla Core Keeper
    /// Simple-Crafting workbenches (Iron / Wood / Copper / Carpenter / …).
    ///
    /// <para>The Vanilla <c>SimpleCraftingUIContainer</c> has an editor-fixed
    /// pool of 3 <c>SimpleCraftingUI</c> entries. When a mod adds an item to
    /// a Vanilla workbench's <c>canCraftObjects</c>, the workbench needs to
    /// render a 4th window — which the pool can't supply, so the item is
    /// silently dropped with a "Not enough SimpleCraftingUIs" error and a
    /// silently-broken UI.</para>
    ///
    /// <para>This mod patches two methods:</para>
    /// <list type="number">
    /// <item><see cref="SimpleCraftingPoolGrowthPatch"/> — clones the pool
    /// on-demand in <c>ShowCraftingUI</c> up to <c>ModConfig.maxPoolSize</c>
    /// windows.</item>
    /// <item><see cref="CraftingNavUIPositionPatch"/> — extrapolates the
    /// <c>CraftingCategoryNavigationUI</c> horizontal position formula for
    /// counts &gt; 3 (Vanilla only hardcodes 1/2/3).</item>
    /// </list>
    ///
    /// <para>No item-side state, no AssetBundle of its own, no IMod-lifecycle
    /// work beyond the bootstrap log. Harmony attributes auto-register.</para>
    /// </summary>
    public sealed class SimpleCraftingPoolExtenderMod : IMod
    {
        public void EarlyInit()
        {
            Debug.Log($"[SimpleCraftingPoolExtender] EarlyInit — pool will grow on demand up to {ModConfig.Instance.maxPoolSize} windows");
        }

        public void Init()
        {
            Debug.Log("[SimpleCraftingPoolExtender] Init — Harmony patches active");
        }

        public void ModObjectLoaded(Object obj) { }

        public void Shutdown() { }

        public void Update() { }
    }
}
