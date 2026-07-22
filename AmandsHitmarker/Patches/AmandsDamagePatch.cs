using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using AmandsHitmarker;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;
using UnityEngine;

namespace AmandsHitmarkerPatches
{
    public class AmandsDamagePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(Player), nameof(Player.ApplyDamageInfo), new[] { typeof(DamageInfoStruct), typeof(EBodyPart), typeof(EBodyPartColliderType), typeof(float) });
        }
        [PatchPostfix]
        private static void PatchPostFix(Player __instance, DamageInfoStruct damageInfo, EBodyPart bodyPartType)
        {
            // Temporary old version support code
            bool IsYourPlayerAgresssor = false;
            Player player = Traverse.Create(damageInfo).Field("Player").GetValue<object>() as Player;
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
            if (IsYourPlayerAgresssor)
            {
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
                if ((AHitmarkerPlugin.EnableDamageNumber.Value && damageInfo.DidBodyDamage > 0.01f) || (AHitmarkerPlugin.EnableArmorDamageNumber.Value && AmandsHitmarkerClass.ArmorDamageNumber > 0.01f))
                {
                    string text = "";
                    AmandsHitmarkerClass.DamageNumber += damageInfo.DidBodyDamage;
                    if (AHitmarkerPlugin.EnableDamageNumber.Value && AmandsHitmarkerClass.DamageNumber > 0.01f)
                    {
                        text = ((int)AmandsHitmarkerClass.DamageNumber).ToString() + " ";
                    }
                    if (AHitmarkerPlugin.EnableArmorDamageNumber.Value && AmandsHitmarkerClass.ArmorDamageNumber > 0.01f)
                    {
                        text = text + "<color=#" + ColorUtility.ToHtmlStringRGB(AHitmarkerPlugin.ArmorColor.Value) + ">" + (Math.Round(AmandsHitmarkerClass.ArmorDamageNumber, 1)).ToString("F1") + "</color> ";
                    }
                    AmandsHitmarkerClass.damageNumberTextMeshPro.text = text;
                    AmandsHitmarkerClass.damageNumberTextMeshPro.color = AHitmarkerPlugin.HitmarkerColor.Value;
                    AmandsHitmarkerClass.damageNumberTextMeshPro.alpha = 1f;
                    AmandsHitmarkerClass.UpdateDamageNumber = false;
                }
            }
            else
            {
                if (AmandsHitmarkerClass.Player != null && __instance == AmandsHitmarkerClass.Player && AHitmarkerPlugin.EnableDamageIndicator.Value && AmandsHitmarkerClass.amandsDamageIndicator != null)
                {
                    AmandsHitmarkerClass.amandsDamageIndicator.SetLocation(damageInfo.MasterOrigin);
                }
            }
        }
    }
}
