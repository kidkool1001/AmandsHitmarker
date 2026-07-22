using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using AmandsHitmarker;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace AmandsHitmarkerPatches
{
    public class AmandsPlayerPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(Player), nameof(Player.Init));
        }
        [PatchPostfix]
        private static void PatchPostFix(ref Player __instance)
        {
            if (__instance != null && __instance.IsYourPlayer)
            {
                AmandsHitmarkerClass.Player = __instance;
                AmandsHitmarkerClass.playerNickname = __instance.Profile.Nickname;
                AmandsHitmarkerClass.PlayerSuperior = __instance.gameObject;
                AmandsHitmarkerClass.Kills = 0;
                AmandsHitmarkerClass.ReloadFiles();
                AmandsHitmarkerClass.CanDebugReloadFiles = true;
            }
        }
    }
}
