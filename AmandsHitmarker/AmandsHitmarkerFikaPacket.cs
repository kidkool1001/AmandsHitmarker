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
public class HitmarkerPacket : INetSerializable
{
    public string AttackerId;
    public string VictimId;
    public EBodyPart BodyPart;
    public float Damage;
    public float ArmorDamage;
    public EDamageType DamageType;
    public Vector3 TargetPosition;
    public Vector3 MasterOrigin;
    public Vector3 HitPoint;
    

    public void Serialize(NetDataWriter writer)
    {
        writer.Put(AttackerId);
        writer.Put(VictimId);
        writer.Put((int)BodyPart);
        writer.Put(Damage);
        writer.Put(ArmorDamage);
        writer.Put((int)DamageType);

        // Vector3 TargetPosition
        writer.Put(TargetPosition.x);
        writer.Put(TargetPosition.y);
        writer.Put(TargetPosition.z);

        // Vector3 MasterOrigin
        writer.Put(MasterOrigin.x);
        writer.Put(MasterOrigin.y);
        writer.Put(MasterOrigin.z);

        // Vector3 HitPoint
        writer.Put(HitPoint.x);
        writer.Put(HitPoint.y);
        writer.Put(HitPoint.z);
    }

    public void Deserialize(NetDataReader reader)
    {
        AttackerId = reader.GetString();
        VictimId = reader.GetString();
        BodyPart = (EBodyPart)reader.GetInt();
        Damage = reader.GetFloat();
        ArmorDamage = reader.GetFloat();
        DamageType = (EDamageType)reader.GetInt();

        // Vector3 TargetPosition
        TargetPosition = new Vector3(reader.GetFloat(), reader.GetFloat(), reader.GetFloat());

        // Vector3 MasterOrigin
        MasterOrigin = new Vector3(reader.GetFloat(), reader.GetFloat(), reader.GetFloat());

        // Vector3 HitPoint
        HitPoint = new Vector3(reader.GetFloat(), reader.GetFloat(), reader.GetFloat());
    }
}
