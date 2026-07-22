using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using AmandsHitmarker;
using EFT;
using Fika.Core.Main.Players;
using Fika.Core.Main.Utils;
using HarmonyLib;
using SPT.Reflection.Patching;
using UnityEngine;

namespace AmandsHitmarkerPatches
{
    public class AmandsKillPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(Player), nameof(Player.OnBeenKilledByAggressor));
        }
        [PatchPostfix]
        private static void PatchPostFix(ref Player __instance, Player aggressor, DamageInfoStruct damageInfo, EBodyPart bodyPart, EDamageType lethalDamageType)
        {
            if (AmandsHitmarkerClass.Player != null && aggressor == AmandsHitmarkerClass.Player && __instance != AmandsHitmarkerClass.Player)
            {
                float distance = Vector3.Distance(aggressor.Position, __instance.Position);
                if (distance < AHitmarkerPlugin.StartDistance.Value || distance > AHitmarkerPlugin.EndDistance.Value) return;
                AmandsHitmarkerClass.killHitmarker = true;
                AmandsHitmarkerClass.killDamageInfo = damageInfo;
                AmandsHitmarkerClass.killBodyPart = bodyPart;
                AmandsHitmarkerClass.killRole = Traverse.Create(Traverse.Create(__instance.Profile.Info).Field("Settings").GetValue<object>()).Field("Role").GetValue<WildSpawnType>();
                AmandsHitmarkerClass.killExperience = aggressor.Profile.EftStats.SessionCounters.GetInt(SessionCounterTypesAbstractClass.ExpKillBase) - AmandsHitmarkerClass.lastSessionXp;
                AmandsHitmarkerClass.lastSessionXp = aggressor.Profile.EftStats.SessionCounters.GetInt(SessionCounterTypesAbstractClass.ExpKillBase);
                AmandsHitmarkerClass.killPlayerName = __instance.Profile.Nickname;
                AmandsHitmarkerClass.killPlayerSide = __instance.Side;
                AmandsHitmarkerClass.killDistance = distance;
                AmandsHitmarkerClass.killLethalDamageType = lethalDamageType;
                AmandsHitmarkerClass.killLevel = __instance.Profile.Info.Level;
                AmandsHitmarkerClass.killWeaponName = damageInfo.Weapon == null ? "?" : AmandsHitmarkerHelper.Localized(damageInfo.Weapon.ShortName, 0);
                AmandsHitmarkerClass.Kills += 1;
                AmandsHitmarkerClass.Killfeed();
                AmandsHitmarkerClass.MultiKillfeed();
            }
            if (AHitmarkerPlugin.EnableRaidKillfeed.Value && aggressor != null)
            {
                if (AmandsHitmarkerClass.Player != null && AmandsHitmarkerClass.Player != aggressor && Vector3.Distance(AmandsHitmarkerClass.Player.Position, __instance.Position) > AHitmarkerPlugin.RaidKillDistance.Value) return;
                AmandsHitmarkerClass.RaidKillfeed(aggressor.Side, Traverse.Create(Traverse.Create(aggressor.Profile.Info).Field("Settings").GetValue<object>()).Field("Role").GetValue<WildSpawnType>(), (aggressor.Side == EPlayerSide.Savage ? AmandsHitmarkerHelper.Transliterate(aggressor.Profile.Nickname) : aggressor.Profile.Nickname), damageInfo.Weapon == null ? "?" : AmandsHitmarkerHelper.Localized(damageInfo.Weapon.ShortName, 0), lethalDamageType, __instance.Side, Traverse.Create(Traverse.Create(__instance.Profile.Info).Field("Settings").GetValue<object>()).Field("Role").GetValue<WildSpawnType>(), (__instance.Side == EPlayerSide.Savage ? AmandsHitmarkerHelper.Transliterate(__instance.Profile.Nickname) : __instance.Profile.Nickname));
            }
        }
    }
}
