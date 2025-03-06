// <copyright file="GameManager.cs" company="Nathan Ford">
// Copyright (c) Nathan Ford. All rights reserved.
// </copyright>

using SandboxParty.Components.State;
using SandboxParty.Resources;

namespace SandboxParty.Managers
{
	public sealed class GameManager : GameObjectSystem<GameManager>, ISceneStartup, ISceneLoadingEvents
	{
		public CameraComponent WorldCamera { get; private set; }

		public BoardGameState BoardState { get; private set; }

		private IReadOnlyDictionary<Scene, MinigameGameState> MinigameStates { get => this.minigameStates; }

		private readonly Dictionary<Scene, MinigameGameState> minigameStates;

		private readonly SceneLoadOptions lobbyOptions;

		private readonly SceneLoadOptions boardOptions;

		private readonly SceneLoadOptions minigameOptions;

		public GameManager(Scene scene) : base(scene)
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
				ShowLoadingScreen = true
			};

			this.minigameOptions = new SceneLoadOptions
			{
				IsAdditive = true,
				DeleteEverything = false,
				ShowLoadingScreen = true
			};

			this.lobbyOptions.SetScene("scenes/start.scene");
		}

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

		void ISceneLoadingEvents.AfterLoad(Scene scene)
		{
			var cameraObject = scene.CreateObject(true);
			this.WorldCamera = cameraObject.AddComponent<CameraComponent>();
			this.WorldCamera.IsMainCamera = true;
			this.WorldCamera.FovAxis = CameraComponent.Axis.Vertical;
			this.WorldCamera.FieldOfView = 70;

			var occlusionObject = this.WorldCamera.AddComponent<AmbientOcclusion>();
			occlusionObject.Intensity = 1;

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
