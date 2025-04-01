// <copyright file="BaseGameState.cs" company="Nathan Ford">
// Copyright (c) Nathan Ford. All rights reserved.
// </copyright>

using System;
using SandboxParty.Components.Character;

namespace SandboxParty.Components.State
{
	public abstract class BaseGameState<TCharacter> : Component, Component.INetworkListener
		where TCharacter : BaseCharacter
	{
		public int MaxPlayers { get; set; } = 8;

		protected abstract GameObject PlayerPrefab { get; }

		protected abstract Vector3 SpawnLocation { get; }

		protected Dictionary<Guid, TCharacter> Characters { get; private set; } = [];

		protected Dictionary<SteamId, TCharacter> OrphanedCharacters { get; private set; } = [];

		bool INetworkListener.AcceptConnection(Connection channel, ref string reason)
		{
			if (Connection.All.Count > MaxPlayers)
				reason += "Max Players";

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

			var player = PlayerPrefab.Clone(SpawnLocation, Rotation.FromYaw(0));
			player.Name = displayName;
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
	}
}
