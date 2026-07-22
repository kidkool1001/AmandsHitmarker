using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using AmandsHitmarker;
using EFT;
using EFT.InventoryLogic;
using HarmonyLib;
using SPT.Reflection.Patching;
using UnityEngine;

namespace AmandsHitmarkerPatches
{
    public class AmandsGameStartedPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(GameWorld), nameof(GameWorld.OnGameStarted));
        }

        [PatchPostfix]
        public static void PatchPostfix(GameWorld __instance)
        {
            var hitmarkerClass = __instance.gameObject.AddComponent<AmandsHitmarkerClass>();
            var hitmarkerAudioSource = __instance.gameObject.AddComponent<AudioSource>();
            hitmarkerClass.Initialize(hitmarkerAudioSource);
        }
    }
}
