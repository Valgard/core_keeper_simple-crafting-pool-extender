using HarmonyLib;
using PugMod;
using Unity.Mathematics;
using UnityEngine;

namespace SimpleCraftingPoolExtender
{
    /// <summary>
    /// Grows <c>SimpleCraftingUIContainer.simpleCraftingUIs</c> on demand
    /// when <c>ShowCraftingUI</c> would otherwise overflow the editor-fixed
    /// pool.
    ///
    /// <para>Logic per workbench open:</para>
    /// <list type="number">
    /// <item>Read the active <c>CraftingCategoryWindowInfo</c> slot-range.</item>
    /// <item>Compute <c>windowsNeeded = ceil(rangeSize / 6)</c>.</item>
    /// <item>If <c>simpleCraftingUIs.Count &lt; min(windowsNeeded, maxPoolSize)</c>,
    /// clone the last existing <c>SimpleCraftingUI</c> and append until the
    /// count matches.</item>
    /// </list>
    ///
    /// <para>The clone inherits the entire child UI tree (recipeUI, outputUI,
    /// title, masking) via <c>UnityEngine.Object.Instantiate</c> — internal
    /// Inspector references re-wire onto the clone's own children. Vanilla
    /// <c>ShowCraftingUI</c> repositions all windows every open
    /// (<c>localPosition.x = -2.5f * (i - (count-1)/2.0)</c>), so the layout
    /// auto-corrects when the new window activates.</para>
    ///
    /// <para>Idempotent — does nothing once the pool already has enough
    /// entries (cheap early-out for subsequent opens).</para>
    ///
    /// <para>Side-effect: any Simple-Crafting workbench whose recipe-buffer
    /// grows past 18 entries will now render up to <c>maxPoolSize</c> windows
    /// instead of erroring. Vanilla CK never exceeds 18, so the path only
    /// fires for mod-added items.</para>
    /// </summary>
    [HarmonyPatch(typeof(SimpleCraftingUIContainer), nameof(SimpleCraftingUIContainer.ShowCraftingUI))]
    internal static class SimpleCraftingPoolGrowthPatch
    {
        [HarmonyPrefix]
        private static void Prefix(SimpleCraftingUIContainer __instance)
        {
            if (__instance.simpleCraftingUIs == null) return;
            if (__instance.simpleCraftingUIs.Count == 0) return; // need a template

            int maxPool = ModConfig.Instance.maxPoolSize;
            if (__instance.simpleCraftingUIs.Count >= maxPool) return;

            // Determine how many windows the active category needs.
            var info = Manager.ui.GetCraftingCategoryWindowInfo();
            if (info == null) return;
            int rangeSize = info.endSlotIndex - info.startSlotIndex + 1;
            if (rangeSize <= 0) return;

            // 6 slots per window; round up.
            int windowsNeeded = (rangeSize + 5) / 6;
            int targetCount = math.min(windowsNeeded, maxPool);
            if (__instance.simpleCraftingUIs.Count >= targetCount) return;

            // Clone template until we reach target.
            int startCount = __instance.simpleCraftingUIs.Count;
            while (__instance.simpleCraftingUIs.Count < targetCount)
            {
                var template = __instance.simpleCraftingUIs[__instance.simpleCraftingUIs.Count - 1];
                if (template == null) break;

                var cloneGO = Object.Instantiate(template.gameObject, template.transform.parent);
                var cloneUI = cloneGO.GetComponent<SimpleCraftingUI>();
                if (cloneUI == null) { Object.Destroy(cloneGO); break; }

                // SimpleCraftingUI.Init() is gated by `inited` (copied to true
                // by Instantiate). Calling it remains future-proof — if a
                // future SDK separates gate from body, this still wires up.
                cloneUI.Init();
                __instance.simpleCraftingUIs.Add(cloneUI);
            }

            Debug.Log($"[SimpleCraftingPoolExtender] Pool grown {startCount} → {__instance.simpleCraftingUIs.Count} (target={targetCount}, rangeSize={rangeSize})");
        }
    }
}
