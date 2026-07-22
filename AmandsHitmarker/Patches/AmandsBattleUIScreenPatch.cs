using System.Reflection;
using AmandsHitmarker;
using EFT;
using EFT.UI;
using HarmonyLib;
using SPT.Reflection.Patching;
using UnityEngine;

namespace AmandsHitmarkerPatches
{
    public class AmandsBattleUIScreenPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(EftBattleUIScreen), "Show", new[] { typeof(GamePlayerOwner) });
        }
        [PatchPostfix]
        private static void PatchPostFix(ref EftBattleUIScreen __instance)
        {
            if (AmandsHitmarkerClass.ActiveUIScreen == __instance.gameObject) return;
            AmandsHitmarkerClass.ActiveUIScreen = __instance.gameObject;
            AmandsHitmarkerClass.DestroyGameObjects();
            AmandsHitmarkerClass.CreateGameObjects(__instance.transform);
            AmandsHitmarkerClass.XPFormula();
            AmandsHitmarkerClass.DebugMode = false;
            AmandsHitmarkerClass.DebugOffset = Vector3.zero;
        }
    }
}
