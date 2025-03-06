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

		protected override void OnUpdate()
		{
			base.OnUpdate();

			var cameraObject = GameManager.Current.WorldCamera.GameObject;
			var cameraFocus = this.CurrentTurn?.WorldPosition ?? Vector3.Zero;
			var newPosition = cameraFocus + new Vector3(-150, 0, 250);
			var newRotation = Rotation.LookAt(cameraFocus + new Vector3(0, 0, 64) - cameraObject.WorldPosition);
			cameraObject.WorldPosition = Vector3.Lerp(cameraObject.WorldPosition, newPosition, Time.Delta * 5f);
			cameraObject.WorldRotation = Rotation.Slerp(cameraObject.WorldRotation, newRotation, Time.Delta * 2.5f);

			if (!this.Network.IsOwner)
				return;

			if (this.Characters.Count > 0)
				this.CurrentTurn ??= this.Characters.ElementAt(this.TurnNumber % this.Characters.Count).Value;
		}

		[Rpc.Host(Flags = NetFlags.HostOnly)]
		void IBoardEvent.OnTurnStarted(BoardCharacter character)
		{
			throw new System.NotImplementedException();
		}

		[Rpc.Host(Flags = NetFlags.HostOnly)]
		void IBoardEvent.OnDestinationReached(BoardCharacter character)
		{
			if (!this.OnDestinationReached_Validate(character))
				return;

			// TODO: Implement OnDestinationReached Handling
			// Instead make the following call only once the player has confirmed that they want to end their turn or it times out. 
			IBoardEvent.Post(x => x.OnTurnEnded(character));
		}

		[Rpc.Host(Flags = NetFlags.HostOnly)]
		void IBoardEvent.OnTurnEnded(BoardCharacter character)
		{
			if (!this.OnTurnEnded_Validate(character))
				return;

			this.TurnNumber++;

			this.CurrentTurn = this.Characters.ElementAt(this.TurnNumber % this.Characters.Count).Value;
		}

		private bool OnDestinationReached_Validate(BoardCharacter character)
		{
			return this.CurrentTurn == character;
		}

		private bool OnTurnEnded_Validate(BoardCharacter character)
		{
			return this.CurrentTurn == character;
		}

		protected override GameObject GetPlayerPrefab()
			=> SceneResource.GetSceneResource(this.Scene, SceneResource.Boards).GetPlayerPrefab();
	}
}
