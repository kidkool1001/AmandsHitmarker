using System;
using System.Reflection;
using AmandsHitmarkerPatches;
using BepInEx;
using BepInEx.Configuration;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;
using UnityEngine;

namespace AmandsHitmarker
{
    [BepInPlugin("com.Amanda.Hitmarker.FikaSync", "HitmarkerFikaSync", "1.0.0")]
    [BepInDependency("com.fika.core", BepInDependency.DependencyFlags.HardDependency)]
    public class AHitmarkerFikaPlugin : BaseUnityPlugin
    {
        private Harmony harmony;
        private bool initialized;
        private EHitmarkerMode currentMode;

        private AmandsFikaPacketsDamagePatch packetsPatch;
        private AmandsFikaLocalDamagePatch localPatch;

        public static ConfigEntry<EHitmarkerMode> HitmarkerMode { get; set; }

        private void Awake()
        {
            harmony = new Harmony("com.Amands.Hitmarker.FikaSync");
        }

        private void Start()
        {
            UnpatchOriginals();

            HitmarkerMode = Config.Bind("AmandsHitmarkerFika", "Hitmarker Mode", EHitmarkerMode.Packets, new ConfigDescription("Whether hitmarkers are sent from host(packets) or client only(local)", null, new ConfigurationManagerAttributes { Order = 421 } ));

            HitmarkerMode.SettingChanged += (_, __) => UpdateHitmarkerMode(HitmarkerMode.Value);

            new AmandsFikaKillPatch().Enable();

            UpdateHitmarkerMode(HitmarkerMode.Value);
            AmandsHitmarkerFikaSync.Init();
        }

        private void UnpatchOriginals()
        {
            var damageMethod = AccessTools.Method(typeof(Player), "ApplyDamageInfo");
            var killMethod = AccessTools.Method(typeof(Player), "OnBeenKilledByAggressor");

            var damageInfo = Harmony.GetPatchInfo(damageMethod);
            if (damageInfo != null)
            {
                foreach (var patch in damageInfo.Postfixes)
                {
                    if (patch.owner != null && patch.owner.Contains("Amands"))
                    {
                        Logger.LogInfo($"Unpatching damage postfix: {patch.owner} -> {patch.PatchMethod?.Name}");
                        harmony.Unpatch(damageMethod, patch.PatchMethod);
                    }
                }
            }
            else
            {
                Logger.LogInfo("No damage patches found.");
            }

            var killInfo = Harmony.GetPatchInfo(killMethod);
            if (killInfo != null)
            {
                foreach (var patch in killInfo.Postfixes)
                {

                    if (patch.owner != null && patch.owner.Contains("Amands"))
                    {
                        Logger.LogInfo($"Unpatching kill postfix: {patch.owner} -> {patch.PatchMethod?.Name}");
                        harmony.Unpatch(killMethod, patch.PatchMethod);
                    }
                }
            }
            else
            {
                Logger.LogInfo("No kill patches found.");
            }
        }

        private void DisableCurrentHitmarkerMode()
        {
            if (packetsPatch != null)
            {
                packetsPatch.Disable();
                packetsPatch = null;
            }

            if (localPatch != null)
            {
                localPatch.Disable();
                localPatch = null;
            }
        }

        private void UpdateHitmarkerMode(EHitmarkerMode newMode)
        {
            if (initialized && currentMode == newMode)
                return;

            DisableCurrentHitmarkerMode();

            switch (newMode)
            {
                case EHitmarkerMode.Packets:
                    packetsPatch = new AmandsFikaPacketsDamagePatch();
                    packetsPatch.Enable();
                    break;

                case EHitmarkerMode.Local:
                    localPatch = new AmandsFikaLocalDamagePatch();
                    localPatch.Enable();
                    break;
            }

            currentMode = newMode;
            initialized = true;
        }

        public enum EHitmarkerMode
        {
            Packets,
            Local
        }
    }
}