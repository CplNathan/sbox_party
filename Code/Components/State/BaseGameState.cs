// <copyright file="BaseGameState.cs" company="Nathan Ford">
// Copyright (c) Nathan Ford. All rights reserved.
// </copyright>

using System;
using SandboxParty.Components.Character;
using SandboxParty.Components.Information;

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

		void INetworkListener.OnActive(Connection channel)
		{
			Log.Info($"Player '{channel.DisplayName}' has joined the game");

			var displayName = $"Player - {channel.DisplayName}";

			if (this.OrphanedCharacters.TryGetValue(channel.SteamId, out TCharacter existingPlayer))
			{
				existingPlayer.GameObject.Name = displayName;
				existingPlayer.GameObject.Enabled = true;
				existingPlayer.GameObject.Network.AssignOwnership(channel);

				this.Characters[channel.Id] = existingPlayer;
				this.OrphanedCharacters.Remove(channel.SteamId);

				return;
			}

			var player = this.GetPlayerPrefab().Clone(this.GetSpawnTransform(), name: displayName);
			player.Network.SetOrphanedMode(NetworkOrphaned.Host);
			player.NetworkSpawn(channel);

			this.Characters[channel.Id] = player.GetComponentInChildren<TCharacter>();
		}

		void INetworkListener.OnDisconnected(Connection channel)
		{
			var character = this.Characters[channel.Id];
			character.GameObject.Enabled = false;

			this.OrphanedCharacters[channel.SteamId] = character;
			this.Characters.Remove(channel.Id);
		}

		protected abstract GameObject GetPlayerPrefab();

		protected Transform GetSpawnTransform()
		{
			return BaseSceneInformation.GetSpawnPoints(this.Scene)[0].WorldTransform;
		}
	}
}
