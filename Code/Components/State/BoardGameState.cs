using SandboxParty.Components.Character.Board;
using SandboxParty.Events;
using SandboxParty.Resources;
using GameManager = SandboxParty.Managers.GameManager;

namespace SandboxParty.Components.State
{
	public sealed class BoardGameState : BaseGameState<BoardCharacter>, IBoardEvent
	{
		[Sync( Flags = SyncFlags.FromHost )] public int TurnNumber { get; private set; } = 0;

		[Sync( Flags = SyncFlags.FromHost )] public BoardCharacter CurrentTurn { get; set; }

		protected override void OnUpdate()
		{
			base.OnUpdate();

			var cameraObject = GameManager.Current.WorldCamera.GameObject;
			var cameraFocus = CurrentTurn?.WorldPosition ?? Vector3.Zero;
			var newPosition = cameraFocus + new Vector3( -150, 0, 275 );
			var newRotation = Rotation.LookAt( cameraFocus + new Vector3( 0, 0, 64 ) - cameraObject.WorldPosition );
			cameraObject.WorldPosition = Vector3.Lerp( cameraObject.WorldPosition, newPosition, Time.Delta * 5f );
			cameraObject.WorldRotation = Rotation.Slerp( cameraObject.WorldRotation, newRotation, Time.Delta * 2.5f );

			if ( !Network.IsOwner )
				return;

			if ( Characters.Count > 0 )
				CurrentTurn ??= Characters.ElementAt( TurnNumber % Characters.Count ).Value;
		}

		[Rpc.Host( Flags = NetFlags.HostOnly )]
		void IBoardEvent.OnTurnStarted( BoardCharacter character )
		{
			throw new System.NotImplementedException();
		}

		[Rpc.Host( Flags = NetFlags.HostOnly )]
		void IBoardEvent.OnDestinationReached( BoardCharacter character )
		{
			if ( !OnDestinationReached_Validate( character ) )
				return;

			// TODO: Implement OnDestinationReached Handling
			// Instead make the following call only once the player has confirmed that they want to end their turn or it times out. 
			IBoardEvent.Post( x => x.OnTurnEnded( character ) );
		}

		[Rpc.Host( Flags = NetFlags.HostOnly )]
		void IBoardEvent.OnTurnEnded( BoardCharacter character )
		{
			if ( !OnTurnEnded_Validate( character ) )
				return;

			TurnNumber++;

			CurrentTurn = Characters.ElementAt( TurnNumber % Characters.Count ).Value;
		}

		private bool OnDestinationReached_Validate( BoardCharacter character )
		{
			return CurrentTurn == character;
		}

		private bool OnTurnEnded_Validate( BoardCharacter character )
		{
			return CurrentTurn == character;
		}

		protected override GameObject GetPlayerPrefab()
			=> SceneResource.GetSceneResource( Scene, SceneResource.Boards ).GetPlayerPrefab();
	}
}
