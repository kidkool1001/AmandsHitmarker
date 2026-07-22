using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using AmandsHitmarker;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace AmandsHitmarkerPatches
{
    public class AmandsSSAAPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(SSAA), "Awake", Type.EmptyTypes);
        }
        [PatchPostfix]
        private static void PatchPostFix(ref SSAA __instance)
        {
            AmandsHitmarkerClass.FPSCameraSSAA = __instance;
        }
    }
}
