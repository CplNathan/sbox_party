using SandboxParty.Components.State;
using SandboxParty.Resources;
using System.Collections.ObjectModel;

namespace SandboxParty.Managers
{
	public sealed class GameManager : GameObjectSystem<GameManager>, ISceneStartup, ISceneLoadingEvents
	{
		public CameraComponent WorldCamera { get; private set; }

		public BoardGameState BoardState { get; private set; }

		private IReadOnlyDictionary<Scene, MinigameGameState> MinigameStates { get => minigameStates; }

		private readonly Dictionary<Scene, MinigameGameState> minigameStates;

		private readonly SceneLoadOptions lobbyOptions;

		private readonly SceneLoadOptions boardOptions;

		private readonly SceneLoadOptions minigameOptions;

		public GameManager( Scene scene ) : base( scene )
		{
			lobbyOptions = new SceneLoadOptions
			{
				IsAdditive = false,
				DeleteEverything = true,
			};

			boardOptions = new SceneLoadOptions
			{
				IsAdditive = false,
				DeleteEverything = false,
				ShowLoadingScreen = true
			};

			minigameOptions = new SceneLoadOptions
			{
				IsAdditive = true,
				DeleteEverything = false,
				ShowLoadingScreen = true
			};

			lobbyOptions.SetScene( "scenes/start.scene" );
		}

		void ISceneStartup.OnHostInitialize()
		{
			Scene.Load( lobbyOptions );

			if ( !Networking.IsActive )
			{
				LoadingScreen.Title = "Creating Lobby";
				Networking.CreateLobby( new() );
			}

			LoadBoard();
		}

		void ISceneLoadingEvents.AfterLoad( Scene scene )
		{
			WorldCamera = scene.CreateObject( true ).AddComponent<CameraComponent>();
			WorldCamera.IsMainCamera = true;
			WorldCamera.FovAxis = CameraComponent.Axis.Vertical;
			WorldCamera.FieldOfView = 70;

			BoardState = scene.GetComponentInChildren<BoardGameState>();
		}

		private void LoadBoard()
		{
			var board = SceneResource.Boards[0];

			boardOptions.SetScene( board.Scene );
			Scene.Load( boardOptions );

			var stateObject = Scene.CreateObject();
			BoardState = stateObject.AddComponent<BoardGameState>();
			stateObject.NetworkSpawn();
		}
	}
}
