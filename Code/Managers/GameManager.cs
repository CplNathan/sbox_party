// <copyright file="GameManager.cs" company="Nathan Ford">
// Copyright (c) Nathan Ford. All rights reserved.
// </copyright>

using SandboxParty.Components.State;
using SandboxParty.Resources;

namespace SandboxParty.Managers
{
	public sealed class GameManager : GameObjectSystem<GameManager>, ISceneStartup, ISceneLoadingEvents
	{
		private readonly Dictionary<Scene, MinigameGameState> minigameStates;

		private readonly SceneLoadOptions lobbyOptions;

		private readonly SceneLoadOptions boardOptions;

		private readonly SceneLoadOptions minigameOptions;

		/// <summary>
		/// Initializes a new instance of the <see cref="GameManager"/> class.
		/// Setup scene loading options.
		/// </summary>
		/// <param name="scene">The initial scene.</param>
		public GameManager(Scene scene)
			: base(scene)
		{
			this.lobbyOptions = new SceneLoadOptions
			{
				IsAdditive = false,
				DeleteEverything = true,
			};

			this.boardOptions = new SceneLoadOptions
			{
				IsAdditive = false,
				DeleteEverything = false,
				ShowLoadingScreen = true,
			};

			this.minigameOptions = new SceneLoadOptions
			{
				IsAdditive = true,
				DeleteEverything = false,
				ShowLoadingScreen = true,
			};

			this.lobbyOptions.SetScene("scenes/start.scene");
		}

		public CameraComponent WorldCamera { get; private set; }

		public BoardGameState BoardState { get; private set; }

		private IReadOnlyDictionary<Scene, MinigameGameState> MinigameStates { get => this.minigameStates; }

		/// <summary>
		/// Called on the host to initialize the lobby and load the game.
		/// </summary>
		void ISceneStartup.OnHostInitialize()
		{
			this.Scene.Load(this.lobbyOptions);

			if (!Networking.IsActive)
			{
				LoadingScreen.Title = "Creating Lobby";
				Networking.CreateLobby(new());
			}

			this.LoadBoard();
		}

		/// <summary>
		/// Called on all clients to sync GameManager once the scene has loaded.
		/// </summary>
		/// <param name="scene">The current scene after it has loaded.</param>
		void ISceneLoadingEvents.AfterLoad(Scene scene)
		{
			var cameraObject = scene.CreateObject(true);
			this.WorldCamera = cameraObject.AddComponent<CameraComponent>();
			this.WorldCamera.IsMainCamera = true;
			this.WorldCamera.FovAxis = CameraComponent.Axis.Vertical;
			this.WorldCamera.FieldOfView = 70;

			var occlusionComponent = this.WorldCamera.AddComponent<AmbientOcclusion>();
			occlusionComponent.Intensity = 1;

			this.BoardState = scene.GetComponentInChildren<BoardGameState>();
		}

		private void LoadBoard()
		{
			var board = SceneResource.Boards[0];

			this.boardOptions.SetScene(board.Scene);
			this.Scene.Load(this.boardOptions);

			var stateObject = this.Scene.CreateObject();
			this.BoardState = stateObject.AddComponent<BoardGameState>();
			stateObject.NetworkSpawn();
		}
	}
}
