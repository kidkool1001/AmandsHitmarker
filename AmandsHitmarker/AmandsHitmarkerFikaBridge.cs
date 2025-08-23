using AmandsHitmarker;
using Comfort.Common;
using EFT;
using Fika.Core.Coop.Utils;
using Fika.Core.Modding.Events;
using Fika.Core.Networking;
using Fika.Core.Modding;
using UnityEngine;
using System;
using EFT.InventoryLogic;
using Fika.Core.Coop.GameMode;
using Fika.Core.Coop.Players;
using Fika.Core;

public static class AmandsHitmarkerFikaBridge
{
    private static bool _initialized = false;

    public static void Init()
    {
        if (_initialized) return;
        _initialized = true;

        FikaEventDispatcher.SubscribeEvent<FikaNetworkManagerCreatedEvent>(evt =>
        {
            if (!FikaBackendUtils.IsClient) return;
            evt.Manager.RegisterPacket<KillPacket>(packet =>
            {
                bool isLocalPlayerAggressor = AmandsHitmarkerClass.Player != null &&
                                              string.Equals(AmandsHitmarkerClass.Player.ProfileId?.Trim(),
                                                            packet.AggressorProfileId?.Trim(),
                                                            StringComparison.OrdinalIgnoreCase);

                if (isLocalPlayerAggressor)
                {
                    AmandsHitmarkerClass.killPlayerName = packet.VictimName;
                    AmandsHitmarkerClass.killPlayerSide = packet.VictimSide;
                    AmandsHitmarkerClass.killRole = packet.VictimRole;
                    AmandsHitmarkerClass.killLevel = packet.VictimLevel;
                    AmandsHitmarkerClass.killWeaponName = packet.WeaponName;
                    AmandsHitmarkerClass.killBodyPart = packet.BodyPart;
                    AmandsHitmarkerClass.killLethalDamageType = packet.LethalDamageType;
                    AmandsHitmarkerClass.killDistance = packet.Distance;
                    AmandsHitmarkerClass.Killfeed();
                    AmandsHitmarkerClass.MultiKillfeed();

                }
                if (AHitmarkerPlugin.EnableRaidKillfeed.Value)
                {
                    AmandsHitmarkerClass.RaidKillfeed(
                        packet.AggressorSide,
                        packet.AggressorRole,
                        packet.AggressorName,
                        packet.WeaponName,
                        packet.LethalDamageType,
                        packet.VictimSide,
                        packet.VictimRole,
                        packet.VictimName
                    );
                }
            });
            evt.Manager.RegisterPacket<HitmarkerPacket>(packet =>
            {
                bool isLocalPlayerAggressor = AmandsHitmarkerClass.Player != null &&
                                              string.Equals(AmandsHitmarkerClass.Player.ProfileId?.Trim(),
                                                            packet.AttackerId?.Trim(),
                                                            StringComparison.OrdinalIgnoreCase);

                bool isLocalPlayerVictim = AmandsHitmarkerClass.Player != null &&
                                              string.Equals(AmandsHitmarkerClass.Player.ProfileId?.Trim(),
                                                            packet.VictimId?.Trim(),
                                                            StringComparison.OrdinalIgnoreCase);

                if (!isLocalPlayerAggressor)
                    return;
                DamageInfoStruct damageInfo = new DamageInfoStruct
                {
                    HitPoint = packet.HitPoint,
                    Damage = packet.Damage,
                    DamageType = packet.DamageType,
                    ArmorDamage = packet.ArmorDamage
                };
                if (AmandsHitmarkerClass.Player != null )
                {
                    float distance = Vector3.Distance(AmandsHitmarkerClass.Player.Position, packet.TargetPosition);
                    if (distance < AHitmarkerPlugin.StartDistance.Value || distance > AHitmarkerPlugin.EndDistance.Value)
                    {
                        AmandsHitmarkerClass.armorHitmarker = false;
                        AmandsHitmarkerClass.armorBreak = false;
                        return;
                    }
                }
                AmandsHitmarkerClass.hitmarker = true;
                AmandsHitmarkerClass.damageInfo = damageInfo;
                AmandsHitmarkerClass.bodyPart = packet.BodyPart;
                if (AmandsHitmarkerClass.damageNumberTextMeshPro == null) return;
                if ((AHitmarkerPlugin.EnableDamageNumber.Value && packet.Damage > 0.01f) || (AHitmarkerPlugin.EnableArmorDamageNumber.Value && packet.ArmorDamage > 0.01f))
                {
                    string text = "";
                    AmandsHitmarkerClass.DamageNumber += packet.Damage;
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
                else
                {
                    if (AmandsHitmarkerClass.Player != null && isLocalPlayerVictim && AHitmarkerPlugin.EnableDamageIndicator.Value && AmandsHitmarkerClass.amandsDamageIndicator != null)
                    {
                        AmandsHitmarkerClass.amandsDamageIndicator.SetLocation(packet.MasterOrigin);
                    }
                }
            });
        });
    }
    public static void SendKill(Player aggressor, Player victim, DamageInfoStruct damageInfo,
                                EBodyPart bodyPart, EDamageType lethalDamageType)
    {
        if (!FikaBackendUtils.IsServer) return;

        var packet = new KillPacket
        {
            AggressorProfileId = aggressor.ProfileId,
            AggressorSide = aggressor.Side,
            AggressorRole = aggressor.Profile.Info.Settings.Role,
            AggressorName = AmandsHitmarkerHelper.Transliterate(aggressor.Profile.Nickname),

            VictimName = victim.Side == EPlayerSide.Savage
                ? AmandsHitmarkerHelper.Transliterate(victim.Profile.Nickname)
                : victim.Profile.Nickname,
            VictimSide = victim.Side,
            VictimRole = victim.Profile.Info.Settings.Role,
            VictimLevel = victim.Profile.Info.Level,

            WeaponName = damageInfo.Weapon == null
                ? "?"
                : AmandsHitmarkerHelper.Localized(damageInfo.Weapon.ShortName, 0),
            BodyPart = bodyPart,
            LethalDamageType = lethalDamageType,
            Distance = Vector3.Distance(aggressor.Position, victim.Position),
        };

        if (Singleton<IFikaNetworkManager>.Instance is FikaServer server)
        {
            server.SendDataToAll(ref packet, LiteNetLib.DeliveryMethod.ReliableOrdered);
        }
    }
    public static void SendHitmarker(Player aggressor, Player victim, DamageInfoStruct damageInfo, EBodyPart bodyPart)
    {
        if (!FikaBackendUtils.IsServer) return;

        if (aggressor == null || victim == null) return;

        var packet = new HitmarkerPacket
        {
            AttackerId = aggressor.ProfileId,
            VictimId = victim.ProfileId,
            BodyPart = bodyPart,
            Damage = damageInfo.DidBodyDamage,
            ArmorDamage = damageInfo.DidArmorDamage,
            TargetPosition = victim.Position,
            MasterOrigin = damageInfo.MasterOrigin,
            HitPoint = damageInfo.HitPoint,
            DamageType = damageInfo.DamageType
        };

        if (Singleton<IFikaNetworkManager>.Instance is FikaServer server)
        {
            server.SendDataToAll(ref packet, LiteNetLib.DeliveryMethod.Unreliable);
        }
    }

}
