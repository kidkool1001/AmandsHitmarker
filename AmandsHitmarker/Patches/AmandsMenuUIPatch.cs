using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using AmandsHitmarker;
using HarmonyLib;
using SPT.Reflection.Patching;
using EFT.UI;

namespace AmandsHitmarkerPatches
{
    public class AmandsMenuUIPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(MenuUI), nameof(MenuUI.Awake));
        }
        [PatchPostfix]
        private static void PatchPostFix(ref MenuUI __instance)
        {
            if (AmandsHitmarkerClass.ActiveUIScreen == __instance.transform.GetChild(0).gameObject) return;
            AmandsHitmarkerClass.ActiveUIScreen = __instance.transform.GetChild(0).gameObject;
            AmandsHitmarkerClass.DestroyGameObjects();
            AmandsHitmarkerClass.CreateGameObjects(__instance.transform.GetChild(0));
            AmandsHitmarkerClass.XPFormula();
        }
    }
}
