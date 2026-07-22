using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using AmandsHitmarker;
using EFT.InventoryLogic;
using HarmonyLib;
using SPT.Reflection.Patching;
using UnityEngine;

namespace AmandsHitmarkerPatches
{
    public class AmandsArmorDamagePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(ArmorComponent), nameof(ArmorComponent.ApplyDurabilityDamage));
        }
        [PatchPrefix]
        private static void PatchPrefix(ref ArmorComponent __instance, float armorDamage, List<ArmorComponent> armorComponents)
        {
            if (armorDamage > 0)
            {
                if (armorComponents.Contains(__instance))
                {
                    AmandsHitmarkerClass.ArmorDamageNumber += Mathf.Min(__instance.Repairable.Durability, armorDamage);
                    AmandsHitmarkerClass.armorHitmarker = true;
                    if (__instance.Repairable.Durability - armorDamage < 0)
                    {
                        AmandsHitmarkerClass.armorBreak = true;
                    }
                }
            }
        }
    }
}
