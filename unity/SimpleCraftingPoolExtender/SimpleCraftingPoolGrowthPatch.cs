using HarmonyLib;
using UnityEngine;

namespace SimpleCraftingPoolExtender
{
    /// <summary>
    /// Expands <c>SimpleCraftingUIContainer.simpleCraftingUIs</c> from the
    /// editor-fixed 3-slot pool to <c>ModConfig.maxPoolSize</c> entries, so
    /// any Simple-Crafting workbench whose mod-added recipes overflow the
    /// vanilla 18-slot limit can render in 4+ windows instead of dropping
    /// the overflow.
    ///
    /// <para><b>Why Awake-Postfix, not ShowCraftingUI-Prefix.</b>
    /// Awake runs <i>once</i>, immediately after Vanilla's <c>Init()</c> loop
    /// over the 3 original entries and before anything is ever rendered.
    /// Cloning the template at this point captures it in its Inspector-default
    /// "all slots inactive, nothing rendered" state, which is exactly what we
    /// want — the new pool entries start clean and Vanilla's ShowCraftingUI
    /// activates only what it needs per open.</para>
    ///
    /// <para>A ShowCraftingUI-Prefix variant ("grow on demand") was tried first
    /// but proved unsound: by the time the Prefix runs, the template
    /// <c>simpleCraftingUIs[Count-1]</c> may have rendered slots from a
    /// previous workbench open. <c>Object.Instantiate</c> deep-copies the
    /// full visual state — so the clone shows up with the previous render's
    /// item-sprites under the new ones from the current workbench, producing
    /// the "double-rendered" 4th-tab effect on screen.</para>
    ///
    /// <para>The trade-off is that all SimpleCrafting workbenches now have
    /// <c>maxPoolSize</c> pool entries even when only 1-3 are needed. This is
    /// free for Vanilla: Vanilla's <c>ShowCraftingUI</c> iterates
    /// <c>amountOfWindowsShowing</c> and explicitly calls <c>HideCraftingUI()</c>
    /// on every pool entry beyond that index, so unused entries sit inactive
    /// and invisible. No render cost, no UI overhead.</para>
    ///
    /// <para>Idempotent: re-running the Postfix returns immediately once the
    /// pool already has <c>maxPoolSize</c> entries.</para>
    /// </summary>
    [HarmonyPatch(typeof(SimpleCraftingUIContainer), "Awake")]
    internal static class SimpleCraftingPoolGrowthPatch
    {
        [HarmonyPostfix]
        private static void Postfix(SimpleCraftingUIContainer __instance)
        {
            if (__instance.simpleCraftingUIs == null) return;
            if (__instance.simpleCraftingUIs.Count == 0) return; // need a template

            int target = ModConfig.Instance.maxPoolSize;
            if (__instance.simpleCraftingUIs.Count >= target) return;

            int startCount = __instance.simpleCraftingUIs.Count;
            while (__instance.simpleCraftingUIs.Count < target)
            {
                var template = __instance.simpleCraftingUIs[__instance.simpleCraftingUIs.Count - 1];
                if (template == null) break;

                var cloneGO = Object.Instantiate(template.gameObject, template.transform.parent);
                var cloneUI = cloneGO.GetComponent<SimpleCraftingUI>();
                if (cloneUI == null) { Object.Destroy(cloneGO); break; }

                // Init() is gated by `inited` (copied to true via Instantiate),
                // so this short-circuits in practice — but keep the call for
                // forward-compatibility with future SDK changes.
                cloneUI.Init();
                __instance.simpleCraftingUIs.Add(cloneUI);
            }

            Debug.Log($"[SimpleCraftingPoolExtender] Pool expanded {startCount} → {__instance.simpleCraftingUIs.Count}");
        }
    }
}
