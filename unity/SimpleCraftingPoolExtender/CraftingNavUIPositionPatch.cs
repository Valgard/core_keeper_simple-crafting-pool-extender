using HarmonyLib;
using PugMod;
using UnityEngine;

namespace SimpleCraftingPoolExtender
{
    /// <summary>
    /// Repositions <c>CraftingCategoryNavigationUI</c> (the green up/down
    /// arrows + workbench-icon widget) when the pool has been grown past
    /// 3 windows by <see cref="SimpleCraftingPoolGrowthPatch"/>.
    ///
    /// <para>Vanilla <c>LateUpdate</c> has a switch with hardcoded x-positions
    /// only for 1/2/3 windows showing. At 4+ the switch falls through silently
    /// and the navigation widget sticks at whatever position it had last frame
    /// — visually it ends up overlapping the rendered windows instead of
    /// sitting to their left.</para>
    ///
    /// <para>Vanilla values: count=1 → −2.8125, count=2 → −5.3125, count=3 →
    /// −7.8125. The formula <c>−2.5 × count − 0.3125</c> reproduces all three
    /// (verified against the decompile) and extrapolates cleanly to higher
    /// counts. Our Postfix sets it for count &gt;= 4 only; counts 1-3 stay
    /// at the exact Vanilla values.</para>
    /// </summary>
    [HarmonyPatch(typeof(CraftingCategoryNavigationUI), "LateUpdate")]
    internal static class CraftingNavUIPositionPatch
    {
        [HarmonyPostfix]
        private static void Postfix(CraftingCategoryNavigationUI __instance)
        {
            if (__instance.root == null || !__instance.root.activeSelf) return;
            int count = Manager.ui.simpleCraftingUIContainer.amountOfWindowsShowing;
            if (count < 4) return; // Vanilla switch already handled 1-3 correctly.

            __instance.root.transform.localPosition = new Vector3(
                -2.5f * count - 0.3125f, 0f, 0f);
        }
    }
}
