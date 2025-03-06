// <copyright file="BoardGameState.cs" company="Nathan Ford">
// Copyright (c) Nathan Ford. All rights reserved.
// </copyright>

using SandboxParty.Components.Character.Board;
using SandboxParty.Events;
using SandboxParty.Resources;
using GameManager = SandboxParty.Managers.GameManager;

namespace SandboxParty.Components.State
{
	public sealed class BoardGameState : BaseGameState<BoardCharacter>, IBoardEvent
	{
		[Sync(Flags = SyncFlags.FromHost)]
		public int TurnNumber { get; private set; } = 0;

		[Sync(Flags = SyncFlags.FromHost)]
		public BoardCharacter CurrentTurn { get; set; }

		protected override void OnConnected(Connection channel)
		{
			if (this.CurrentTurn == default)
			{
				Log.Warning($"Connected user starting turn {channel.DisplayName}");

				var character = this.Characters[channel.Id];
				IBoardEvent.Post(x => x.OnTurnStarted(character));
			}
		}

		protected override void OnDisconnected(Connection channel)
		{
			if (this.CurrentTurn.Network.Owner == channel)
			{
				Log.Warning($"Disconnected user ending turn {channel.DisplayName}");

				IBoardEvent.Post(x => x.OnTurnEnded(this.CurrentTurn));
			}
		}

		[Rpc.Host(Flags = NetFlags.HostOnly)]
		void IBoardEvent.OnTurnStarted(BoardCharacter character)
		{
			if (!this.OnTurnStarted_Validate(character))
			{
				Log.Warning($"Failed to validate OnTurnStarted RPC for {character.Network.Owner.DisplayName}");
				return;
			}

			Log.Info($"Turn started for {character.Network.Owner.DisplayName}");

			this.TurnNumber++;
			this.CurrentTurn = character;
		}

		[Rpc.Host(Flags = NetFlags.HostOnly)]
		void IBoardEvent.OnDestinationReached(BoardCharacter character)
		{
			if (!this.OnDestinationReached_Validate(character))
			{
				Log.Warning($"Failed to validate OnDestinationReached RPC for {character.Network.Owner.DisplayName}");
				return;
			}

			Log.Info($"Destination reached for {character.Network.Owner.DisplayName}");

			// TODO: Implement OnDestinationReached Handling
			// Instead make the following call only once the player has confirmed that they want to end their turn or it times out.
			IBoardEvent.Post(x => x.OnTurnEnded(character));
		}

		[Rpc.Host(Flags = NetFlags.HostOnly)]
		void IBoardEvent.OnTurnEnded(BoardCharacter character)
		{
			if (!this.OnTurnEnded_Validate(character))
			{
				Log.Warning($"Failed to validate OnTurnEnded RPC for {character.Network.Owner.DisplayName}");
				return;
			}

			Log.Info($"Turn ended for {character.Network.Owner.DisplayName}");

			IBoardEvent.Post(x => x.OnTurnStarted(this.Characters.ElementAt(this.TurnNumber % this.Characters.Count).Value));
		}

		protected override GameObject GetPlayerPrefab()
			=> SceneResource.GetSceneResource(this.Scene, SceneResource.Boards).GetPlayerPrefab();

		protected override void OnUpdate()
		{
			base.OnUpdate();

			var cameraObject = GameManager.Current.WorldCamera.GameObject;
			var cameraFocus = this.CurrentTurn?.WorldPosition ?? Vector3.Zero;
			var newPosition = cameraFocus + new Vector3(-100, 0, 150);
			var newRotation = Rotation.LookAt(cameraFocus + new Vector3(0, 0, 64) - cameraObject.WorldPosition);
			cameraObject.WorldPosition = Vector3.Lerp(cameraObject.WorldPosition, newPosition, Time.Delta * 5f);
			cameraObject.WorldRotation = Rotation.Slerp(cameraObject.WorldRotation, newRotation, Time.Delta * 2.5f);
		}

		private bool OnTurnStarted_Validate(BoardCharacter character)
		{
			if (!Rpc.Caller.IsHost)
			{
				return false;
			}

			return true;
		}

		private bool OnDestinationReached_Validate(BoardCharacter character)
		{
			if (!Rpc.Caller.IsHost)
			{
				return false;
			}

			return this.CurrentTurn == character;
		}

		private bool OnTurnEnded_Validate(BoardCharacter character)
		{
			if (!Rpc.Caller.IsHost)
			{
				return false;
			}

			return this.CurrentTurn == character;
		}
	}
}
