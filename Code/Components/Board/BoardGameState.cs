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

		private Dictionary<Guid, BoardCharacter> Characters { get; set; } = [];

		private Dictionary<SteamId, BoardCharacter> OrphanedCharacters { get; set; } = [];

		private CameraComponent _sceneCamera;

		protected override void OnStart()
		{
			base.OnStart();

			var cameraGameObject = Scene.CreateObject( true );
			_sceneCamera = cameraGameObject.AddComponent<CameraComponent>();
			_sceneCamera.FovAxis = CameraComponent.Axis.Vertical;
			_sceneCamera.FieldOfView = 70;
		}

		protected override void OnUpdate()
		{
			base.OnUpdate();

			var cameraFocus = CurrentTurn?.WorldPosition ?? Vector3.Zero;
			var newPosition = cameraFocus + new Vector3( -200, 0, 175 );
			var newRotation = Rotation.LookAt( cameraFocus + new Vector3( 0, 0, 64 ) - _sceneCamera.WorldPosition );
			_sceneCamera.WorldPosition = Vector3.Lerp( _sceneCamera.WorldPosition, newPosition, Time.Delta * 5f );
			_sceneCamera.WorldRotation = Rotation.Slerp( _sceneCamera.WorldRotation, newRotation, Time.Delta * 2.5f );

			if ( !Network.IsOwner )
				return;

			if ( Characters.Count > 0 )
				CurrentTurn ??= Characters.ElementAt( TurnNumber % Characters.Count ).Value;
		}

		public void OnPlayerJoined( Connection channel, GameObject playerPrefab, Transform startLocation )
		{
			var displayName = $"Player - {channel.DisplayName}";

			if ( OrphanedCharacters.TryGetValue( channel.SteamId, out BoardCharacter existingPlayer ) )
			{
				existingPlayer.GameObject.Name = displayName;
				existingPlayer.GameObject.Enabled = true;
				existingPlayer.GameObject.Network.AssignOwnership( channel );

				Characters[channel.Id] = existingPlayer;
				OrphanedCharacters.Remove( channel.SteamId );

				return;
			}

			var player = playerPrefab.Clone( startLocation, name: displayName );
			player.Network.SetOrphanedMode( NetworkOrphaned.ClearOwner );
			player.NetworkSpawn( channel );

			Characters[channel.Id] = player.GetComponentInChildren<BoardCharacter>();
		}

		public void OnPlayerLeft( Connection channel )
		{
			var character = Characters[channel.Id];
			character.GameObject.Enabled = false;

			OrphanedCharacters[channel.SteamId] = character;
			Characters.Remove( channel.Id );

			if ( CurrentTurn == character )
				OnTurnEnded( character );
		}

		[Rpc.Host( Flags = NetFlags.HostOnly )]
		public void OnDestinationReached( BoardCharacter character )
		{
			if ( !OnDestinationReached_Validate( character ) )
				return;

			IBoardGameEvent.Post( x => x.OnTurnEnded( character ) );
		}

		public bool OnDestinationReached_Validate( BoardCharacter character )
		{
			return CurrentTurn == character;
		}

		[Rpc.Host( Flags = NetFlags.HostOnly )]
		public void OnTurnEnded( BoardCharacter character )
		{
			if ( !OnTurnEnded_Validate( character ) )
				return;

			TurnNumber++;

			CurrentTurn = Characters.ElementAt( TurnNumber % Characters.Count ).Value;
		}

		public bool OnTurnEnded_Validate( BoardCharacter character )
		{
			return CurrentTurn == character;
		}
	}
}
