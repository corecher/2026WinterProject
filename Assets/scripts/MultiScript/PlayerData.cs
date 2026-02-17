using Unity.Netcode;
using Unity.Collections; // FixedString 사용을 위해 필요
using System;

// 네트워크로 전송 가능한 플레이어 데이터 구조체
public struct PlayerData : INetworkSerializable, IEquatable<PlayerData>
{
    public ulong ClientId;
    public FixedString64Bytes PlayerName; // string 대신 FixedString 사용 필수

    // 생성자
    public PlayerData(ulong id, string name)
    {
        ClientId = id;
        PlayerName = new FixedString64Bytes(name);
    }

    // 직렬화 (데이터 포장/풀기)
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref ClientId);
        serializer.SerializeValue(ref PlayerName);
    }

    // 비교 함수 (리스트 내부 동작용)
    public bool Equals(PlayerData other)
    {
        return ClientId == other.ClientId && PlayerName == other.PlayerName;
    }
}
