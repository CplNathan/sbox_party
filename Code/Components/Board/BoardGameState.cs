using Sandbox.Internal;
using SandboxParty.Components.Board.Character;
using SandboxParty.Events;
using System;

namespace SandboxParty.Components.Board
{
	public enum PlayingState
	{
		Lobby,
		Playing,
		Minigame,
		Finished
	}

	public class BoardGameState : Component, Component.INetworkListener, IBoardGameEvent
	{
		[Sync( Flags = SyncFlags.FromHost )] public int TurnNumber { get; private set; } = 0;

		[Sync( Flags = SyncFlags.FromHost )] public PlayingState CurrentState { get; private set; }

		[Sync( Flags = SyncFlags.FromHost )] public BoardCharacter CurrentTurn { get; private set; }

		[Sync( Flags = SyncFlags.FromHost )] public NetDictionary<Guid, BoardCharacter> Players { get; private set; } = [];

		public void OnTurnEnded( BoardCharacter character )
		{
			if ( !OnTurnEnded_Validate( character ) )
				return;

			TurnNumber++;

			CurrentTurn = Players.ElementAt( TurnNumber % Players.Count ).Value;
		}

		public bool OnTurnEnded_Validate( BoardCharacter character )
		{
			return CurrentTurn == character;
		}

		protected override void OnUpdate()
		{
			base.OnUpdate();

			if ( !Network.IsOwner )
				return;

			CurrentTurn ??= Players.ElementAt( TurnNumber % Players.Count ).Value;
		}
	}
}
