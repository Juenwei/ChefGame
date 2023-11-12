using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public struct PlayerData : IEquatable<PlayerData>, INetworkSerializable
{
	public ulong clientId;
	public int colorId;
	public FixedString64Bytes playerName;
	public FixedString64Bytes playerLobbyId;

	public bool Equals(PlayerData other)
	{
		return clientId == other.clientId && colorId == other.colorId && playerName == other.playerName 
			&& playerLobbyId == other.playerLobbyId;
	}

	public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
	{
		serializer.SerializeValue(ref clientId);
		serializer.SerializeValue(ref colorId);
		serializer.SerializeValue(ref playerName);
		serializer.SerializeValue(ref playerLobbyId);
	}
}
