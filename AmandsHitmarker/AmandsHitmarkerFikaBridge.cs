using AmandsHitmarker;
using Comfort.Common;
using EFT;
using Fika.Core.Main.Utils;
using Fika.Core.Modding.Events;
using Fika.Core.Networking;
using Fika.Core.Modding;
using UnityEngine;
using System;
using EFT.InventoryLogic;
using Fika.Core.Main.GameMode;
using Fika.Core.Main.Players;
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
                    AmandsHitmarkerClass.killWeaponName = AmandsHitmarkerHelper.Localized(packet.WeaponName, 0);
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
                        AmandsHitmarkerHelper.Localized(packet.WeaponName, 0),
                        packet.LethalDamageType,
                        packet.VictimSide,
                        packet.VictimRole,
                        packet.VictimName
                    );
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

            WeaponName = damageInfo.Weapon.ShortName,
            BodyPart = bodyPart,
            LethalDamageType = lethalDamageType,
            Distance = Vector3.Distance(aggressor.Position, victim.Position),
        };

        if (Singleton<IFikaNetworkManager>.Instance is FikaServer server)
        {
            server.SendData(ref packet, Fika.Core.Networking.LiteNetLib.DeliveryMethod.ReliableOrdered);
        }
    }
}
