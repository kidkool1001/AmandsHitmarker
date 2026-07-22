using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using AmandsHitmarker;
using Comfort.Common;
using EFT;
using Fika.Core.Main.Utils;
using HarmonyLib;
using SPT.Reflection.Patching;
using UnityEngine;

namespace AmandsHitmarkerPatches
{
    public class AmandsFikaPacketsDamagePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(Player), nameof(Player.ApplyDamageInfo), new[] { typeof(DamageInfoStruct), typeof(EBodyPart), typeof(EBodyPartColliderType), typeof(float) });
        }

        [PatchPrefix]
        public static void Prefix(Player __instance, DamageInfoStruct damageInfo, EBodyPart bodyPartType, ref float __state)
        {
            if (!FikaBackendUtils.IsServer) return;
            if (__instance?.HealthController == null) return;

            __state = __instance.HealthController.GetBodyPartHealth(bodyPartType).Current;
        }

        [PatchPostfix]
        private static void PatchPostFix(Player __instance, DamageInfoStruct damageInfo, EBodyPart bodyPartType, ref float __state)
        {
            if (!FikaBackendUtils.IsServer) return;
            if (__instance?.HealthController == null) return;

            bool IsYourPlayerAgresssor = false;
            Player player = damageInfo.Player?.iPlayer as Player;
            if (player != null)
            {
                IsYourPlayerAgresssor = AmandsHitmarkerClass.Player != null && player == AmandsHitmarkerClass.Player;
            }
            else
            {
                object playerObject = Traverse.Create(damageInfo).Field("Player").GetValue<object>();
                if (playerObject != null)
                {
                    string AggressorNickname = Traverse.Create(playerObject).Property("Nickname").GetValue<string>();
                    IsYourPlayerAgresssor = AggressorNickname == AmandsHitmarkerClass.playerNickname;
                }
            }

            float postHp = __instance.HealthController.GetBodyPartHealth(bodyPartType).Current;
            float realDamage = Mathf.Max(0f, __state - postHp);      // actual HP removed, capped, no overkill
            float armorDmg = damageInfo.DidArmorDamage;

            if (IsYourPlayerAgresssor)
            {
                if (realDamage <= 0.01f) return;

                if (AmandsHitmarkerClass.Player != null)
                {
                    float distance = Vector3.Distance(AmandsHitmarkerClass.Player.Position, __instance.Position);
                    if (distance < AHitmarkerPlugin.StartDistance.Value || distance > AHitmarkerPlugin.EndDistance.Value)
                    {
                        AmandsHitmarkerClass.armorHitmarker = false;
                        AmandsHitmarkerClass.armorBreak = false;
                        return;
                    }
                }

                AmandsHitmarkerClass.hitmarker = true;
                AmandsHitmarkerClass.damageInfo = damageInfo;
                AmandsHitmarkerClass.bodyPart = bodyPartType;

                if (AmandsHitmarkerClass.damageNumberTextMeshPro == null) return;
                if ((AHitmarkerPlugin.EnableDamageNumber.Value && realDamage > 0.01f)
                    || (AHitmarkerPlugin.EnableArmorDamageNumber.Value && AmandsHitmarkerClass.ArmorDamageNumber > 0.01f))
                {
                    string text = "";
                    AmandsHitmarkerClass.DamageNumber += realDamage;
                    if (AHitmarkerPlugin.EnableDamageNumber.Value && AmandsHitmarkerClass.DamageNumber > 0.01f)
                    {
                        text = ((int)AmandsHitmarkerClass.DamageNumber).ToString() + " ";
                    }
                    if (AHitmarkerPlugin.EnableArmorDamageNumber.Value && AmandsHitmarkerClass.ArmorDamageNumber > 0.01f)
                    {
                        text = text + "<color=#" + ColorUtility.ToHtmlStringRGB(AHitmarkerPlugin.ArmorColor.Value) + ">"
                            + Math.Round(AmandsHitmarkerClass.ArmorDamageNumber, 1).ToString("F1") + "</color> ";
                    }
                    AmandsHitmarkerClass.damageNumberTextMeshPro.text = text;
                    AmandsHitmarkerClass.damageNumberTextMeshPro.color = AHitmarkerPlugin.HitmarkerColor.Value;
                    AmandsHitmarkerClass.damageNumberTextMeshPro.alpha = 1f;
                    AmandsHitmarkerClass.UpdateDamageNumber = false;
                }
            }
            else
            {
                IPlayerOwner playerOwner = damageInfo.Player;
                if (playerOwner == null) return;
                Player aggressor = Singleton<GameWorld>.Instance.GetAlivePlayerByProfileID(playerOwner.iPlayer.ProfileId);
                if (aggressor == null) return;

                if (realDamage <= 0.01f && armorDmg <= 0.01f) return;

                float distance = Vector3.Distance(aggressor.Position, __instance.Position);
                if (distance < AHitmarkerPlugin.StartDistance.Value || distance > AHitmarkerPlugin.EndDistance.Value)
                    return;

                damageInfo.Damage = realDamage;
                AmandsHitmarkerFikaSync.SendHitmarker(aggressor, __instance, damageInfo, bodyPartType);
            }

            if (AmandsHitmarkerClass.Player != null
                && __instance == AmandsHitmarkerClass.Player
                && AHitmarkerPlugin.EnableDamageIndicator.Value
                && AmandsHitmarkerClass.amandsDamageIndicator != null)
            {
                AmandsHitmarkerClass.amandsDamageIndicator.SetLocation(damageInfo.MasterOrigin);
            }
        }
    }
}
