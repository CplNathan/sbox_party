// <copyright file="BoardGameState.cs" company="Nathan Ford">
// Copyright (c) Nathan Ford. All rights reserved.
// </copyright>

using SandboxParty.Components.Character.Board;
using SandboxParty.Components.World;
using SandboxParty.Components.World.Board;
using SandboxParty.Events;
using SandboxParty.Resources;
using GameManager = SandboxParty.Managers.GameManager;

namespace SandboxParty.Components.State
{
	public sealed class BoardGameState : BaseGameState<BoardCharacter>, IBoardTurnEvent
	{
		[Sync(Flags = SyncFlags.FromHost)]
		public int TurnNumber { get; private set; } = 0;

		[Sync(Flags = SyncFlags.FromHost)]
		public BoardCharacter CurrentTurn { get; set; }

		protected override void OnConnected(Connection channel)
		{
			if (CurrentTurn == default || CurrentTurn.IsValid() == false)
			{
				Log.Warning($"Connected user starting turn {channel.DisplayName}");

				var character = Characters[channel.Id];
				IBoardTurnEvent.Post(x => x.OnTurnStarted(character));
			}
		}

		protected override void OnDisconnected(Connection channel)
		{
			if (CurrentTurn.Network.Owner == channel)
			{
				Log.Warning($"Disconnected user ending turn {channel.DisplayName}");

				IBoardTurnEvent.Post(x => x.OnTurnEnded(CurrentTurn));
			}
		}

		[Rpc.Host(Flags = NetFlags.HostOnly)]
		void IBoardTurnEvent.OnTurnStarted(BoardCharacter character)
		{
			if (!OnTurnStarted_Validate(character))
			{
				Log.Warning($"Failed to validate OnTurnStarted RPC for {character.Network.Owner.DisplayName}");
				return;
			}

			Log.Info($"Turn started for {character.Network.Owner.DisplayName}");

			TurnNumber++;
			CurrentTurn = character;
		}

		[Rpc.Host(Flags = NetFlags.HostOnly)]
		void IBoardTurnEvent.OnDestinationReached(BoardCharacter character)
		{
			if (!OnDestinationReached_Validate(character))
			{
				Log.Warning($"Failed to validate OnDestinationReached RPC for {character.Network.Owner.DisplayName}");
				return;
			}

			Log.Info($"Destination reached for {character.Network.Owner.DisplayName}");

			// TODO: Implement OnDestinationReached Handling
			// Instead make the following call only once the player has confirmed that they want to end their turn or it times out.
			IBoardTurnEvent.Post(x => x.OnTurnEnded(character));
		}

		[Rpc.Host(Flags = NetFlags.HostOnly)]
		void IBoardTurnEvent.OnTurnEnded(BoardCharacter character)
		{
			if (!OnTurnEnded_Validate(character))
			{
				Log.Warning($"Failed to validate OnTurnEnded RPC for {character.Network.Owner.DisplayName}");
				return;
			}

			Log.Info($"Turn ended for {character.Network.Owner.DisplayName}");

			if ((TurnNumber % Characters.Count) == 0 && TurnNumber - 1 >= Characters.Count)
			{
				IBoardRoundEvent.Post(x => x.OnRoundEnded());
			}
			else
			{
				IBoardTurnEvent.Post(x => x.OnTurnStarted(Characters.ElementAt(TurnNumber % Characters.Count).Value));
			}
		}

		protected override GameObject GetPlayerPrefab()
			=> SceneResource.GetSceneResource(Scene, SceneResource.Boards).GetPlayerPrefab();

		protected override Vector3 GetSpawnLocation()
		{
			return Scene.GetComponentsInChildren<BoardComponent>()
				.Where(component => component.IsSpawn).First()
				.GameObject
				.WorldPosition;
		}

		protected override void OnUpdate()
		{
			base.OnUpdate();

			var cameraObject = GameManager.Current.WorldCamera.GameObject;
			var cameraFocus = CurrentTurn?.WorldPosition ?? Vector3.Zero;
			var newPosition = cameraFocus + new Vector3(-150, 0, 150);
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

			return CurrentTurn == character;
		}

		private bool OnTurnEnded_Validate(BoardCharacter character)
		{
			if (!Rpc.Caller.IsHost)
			{
				return false;
			}

			return CurrentTurn == character;
		}

		public void OnRoundEnded()
		{
			throw new System.NotImplementedException();
		}
	}
}
