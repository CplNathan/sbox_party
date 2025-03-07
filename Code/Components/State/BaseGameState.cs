// <copyright file="BaseGameState.cs" company="Nathan Ford">
// Copyright (c) Nathan Ford. All rights reserved.
// </copyright>

using System;
using System.Threading.Channels;
using SandboxParty.Components.Character;
using SandboxParty.Components.World;

namespace SandboxParty.Components.State
{
	public abstract class BaseGameState<TCharacter> : Component, Component.INetworkListener
		where TCharacter : BaseCharacter
	{
		protected Dictionary<Guid, TCharacter> Characters { get; private set; } = [];

		protected Dictionary<SteamId, TCharacter> OrphanedCharacters { get; private set; } = [];

		bool INetworkListener.AcceptConnection(Connection channel, ref string reason)
		{
			channel.CanSpawnObjects = false;
			channel.CanRefreshObjects = false;

			return string.IsNullOrWhiteSpace(reason);
		}

		protected abstract void OnConnected(Connection channel);

		protected abstract void OnDisconnected(Connection channel);

		void INetworkListener.OnActive(Connection channel)
		{
			Log.Info($"Player '{channel.DisplayName}' has joined the game");

			var displayName = $"Player - {channel.DisplayName}";

			if (OrphanedCharacters.TryGetValue(channel.SteamId, out TCharacter existingPlayer))
			{
				Log.Info($"Retrieving existing character for {displayName}");
				existingPlayer.GameObject.Name = displayName;
				existingPlayer.GameObject.Enabled = true;
				existingPlayer.GameObject.Network.AssignOwnership(channel);

				Characters[channel.Id] = existingPlayer;
				OrphanedCharacters.Remove(channel.SteamId);

				return;
			}

			Log.Info($"Spawning new character for {displayName}");

			var player = GetPlayerPrefab().Clone(GetSpawnTransform(), name: displayName);
			player.Network.SetOrphanedMode(NetworkOrphaned.Host);
			player.NetworkSpawn(channel);

			Characters[channel.Id] = player.GetComponentInChildren<TCharacter>();

			OnConnected(channel);
		}

		void INetworkListener.OnDisconnected(Connection channel)
		{
			Log.Info($"Storing disconnected character for {channel.DisplayName}");
			var character = Characters[channel.Id];
			character.GameObject.Enabled = false;

			OrphanedCharacters[channel.SteamId] = character;
			Characters.Remove(channel.Id);

			OnDisconnected(channel);
		}

		protected abstract GameObject GetPlayerPrefab();

		protected Transform GetSpawnTransform()
		{
			return BaseSceneInformation.GetSpawnPoints(Scene)[0].WorldTransform.WithScale(1).WithRotation(Rotation.FromYaw(0));
		}
	}
}
