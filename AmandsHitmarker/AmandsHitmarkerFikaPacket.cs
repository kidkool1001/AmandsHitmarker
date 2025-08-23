using EFT;
using LiteNetLib.Utils;
using UnityEngine;

public struct KillPacket : INetSerializable
{
    // Private Kill UI identifier for filtering
    public string AggressorProfileId;
    public EPlayerSide AggressorSide;
    public WildSpawnType AggressorRole;
    public string AggressorName;
    public string VictimName;
    public EPlayerSide VictimSide;
    public WildSpawnType VictimRole;
    public int VictimLevel;
    public string WeaponName;
    public EBodyPart BodyPart;
    public EDamageType LethalDamageType;
    public float Distance;

    public void Serialize(NetDataWriter writer)
    {
        writer.Put(AggressorProfileId ?? "");
        writer.Put((int)AggressorSide);
        writer.Put((int)AggressorRole);
        writer.Put(AggressorName ?? "");
        writer.Put(VictimName ?? "");
        writer.Put((int)VictimSide);
        writer.Put((int)VictimRole);
        writer.Put(VictimLevel);
        writer.Put(WeaponName ?? "?");
        writer.Put((int)BodyPart);
        writer.Put((int)LethalDamageType);
        writer.Put(Distance);
    }

    public void Deserialize(NetDataReader reader)
    {
        AggressorProfileId = reader.GetString();
        AggressorSide = (EPlayerSide)reader.GetInt();
        AggressorRole = (WildSpawnType)reader.GetInt();
        AggressorName = reader.GetString();
        VictimName = reader.GetString();
        VictimSide = (EPlayerSide)reader.GetInt();
        VictimRole = (WildSpawnType)reader.GetInt();
        VictimLevel = reader.GetInt();
        WeaponName = reader.GetString();
        BodyPart = (EBodyPart)reader.GetInt();
        LethalDamageType = (EDamageType)reader.GetInt();
        Distance = reader.GetFloat();
    }
}
