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
    public class AmandsFikaLocalDamagePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(Player), "ShotReactions", new[] { typeof(DamageInfoStruct), typeof(EBodyPart) });
        }
        [PatchPostfix]
        private static void PatchPostFix(ref Player __instance, DamageInfoStruct shot, EBodyPart bodyPart)
        {
            bool IsYourPlayerAgresssor = false;
            Player player = shot.Player?.iPlayer as Player;
            if (player != null)
            {
                IsYourPlayerAgresssor = AmandsHitmarkerClass.Player != null && player == AmandsHitmarkerClass.Player;
            }
            else
            {
                object playerObject = Traverse.Create(shot).Field("Player").GetValue<object>();
                if (playerObject != null)
                {
                    string AggressorNickname = Traverse.Create(playerObject).Property("Nickname").GetValue<string>();
                    IsYourPlayerAgresssor = AggressorNickname == AmandsHitmarkerClass.playerNickname;
                }
            }
            if (IsYourPlayerAgresssor && (FikaBackendUtils.IsClient || FikaBackendUtils.IsServer))
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
                AmandsHitmarkerClass.damageInfo = shot;
                AmandsHitmarkerClass.bodyPart = AmandsHitmarkerClass.bodyPart != EBodyPart.Chest ? AmandsHitmarkerClass.bodyPart : bodyPart;
                if (AmandsHitmarkerClass.damageNumberTextMeshPro == null) return;
                if ((AHitmarkerPlugin.EnableDamageNumber.Value && shot.Damage > 0.01f) || (AHitmarkerPlugin.EnableArmorDamageNumber.Value && AmandsHitmarkerClass.ArmorDamageNumber > 0.01f))
                {
                    string text = "";
                    AmandsHitmarkerClass.DamageNumber += shot.Damage;
                    if (AHitmarkerPlugin.EnableDamageNumber.Value && AmandsHitmarkerClass.DamageNumber > 0.01f)
                    {
                        text = ((int)AmandsHitmarkerClass.DamageNumber).ToString() + " ";
                    }
                    if (AHitmarkerPlugin.EnableArmorDamageNumber.Value && AmandsHitmarkerClass.ArmorDamageNumber > 0.01f)
                    {
                        if (AmandsHitmarkerClass.ArmorDamageNumber > 0.01f)
                        {
                            text = text + "<color=#" + ColorUtility.ToHtmlStringRGB(AHitmarkerPlugin.ArmorColor.Value) + ">" + (Math.Round(AmandsHitmarkerClass.ArmorDamageNumber, 1)).ToString("F1") + "</color> ";
                        }
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
                    AmandsHitmarkerClass.amandsDamageIndicator.SetLocation(shot.MasterOrigin);
                }
            }
        }
    }
}
