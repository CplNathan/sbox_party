// <copyright file="GameManager.cs" company="Nathan Ford">
// Copyright (c) Nathan Ford. All rights reserved.
// </copyright>

using SandboxParty.Components.State;
using SandboxParty.Events;
using SandboxParty.Resources;
using static Sandbox.Services.Leaderboards;

namespace SandboxParty.Managers
{
	public sealed class GameManager : GameObjectSystem<GameManager>, ISceneStartup, ISceneLoadingEvents, IBoardRoundEvent
	{
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
				IsAdditive = false,
				DeleteEverything = true,
				ShowLoadingScreen = true,
			};

			this.lobbyOptions.SetScene("scenes/start.scene");
		}

		public CameraComponent WorldCamera { get; private set; }

		public BoardGameState BoardState { get; private set; }

		private MinigameGameState MinigameState { get; set; }

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
		/// <param name="scene">The current scene after it has been loaded.</param>
		void ISceneLoadingEvents.AfterLoad(Scene scene)
		{
			Log.Info("Syncing client");
			var cameraObject = scene.CreateObject(true);
			this.WorldCamera = cameraObject.AddComponent<CameraComponent>();
			this.WorldCamera.IsMainCamera = true;
			this.WorldCamera.FovAxis = CameraComponent.Axis.Vertical;
			this.WorldCamera.FieldOfView = 90;

			var occlusionComponent = this.WorldCamera.AddComponent<AmbientOcclusion>();
			occlusionComponent.Intensity = 1;

			this.BoardState ??= scene.GetComponentInChildren<BoardGameState>();
			Log.Info($"Synced {this.BoardState}");

			this.MinigameState = scene.GetComponentInChildren<MinigameGameState>();
			Log.Info($"Synced {this.MinigameState}");
		}

		void IBoardRoundEvent.OnRoundEnded()
		{
			if (!OnRoundEnded_Validate())
			{
				Log.Warning($"Failed to validate OnRoundEnded RPC");
				return;
			}

			LoadMinigame();
		}

		private void LoadBoard()
		{
			// TODO: Select board
			var board = SceneResource.Boards[0];

			Log.Info($"Loading board {board.SceneType} {board.Scene}");
			this.boardOptions.SetScene(board.Scene);
			var sceneLoaded = this.Scene.Load(this.boardOptions);

			if (sceneLoaded)
			{
				Log.Info($"Loaded {board.SceneType} {board.Scene}");
				var stateObject = this.Scene.CreateObject();
				this.BoardState = stateObject.AddComponent<BoardGameState>();
				stateObject.NetworkSpawn();
				Log.Info($"Created {nameof(BoardGameState)}");
			}
			else
			{
				Log.Error($"Failed to load {board.SceneType} {board.Scene}!");
			}
		}

		private void LoadMinigame()
		{
			// TODO: Select minigame
			var minigame = SceneResource.Minigames[0];

			Log.Info($"Loading minigame {minigame.SceneType} {minigame.Scene}");
			this.minigameOptions.SetScene(minigame.Scene);
			var sceneLoaded = this.Scene.Load(this.minigameOptions);

			if (sceneLoaded)
			{
				Log.Info($"Loaded {minigame.SceneType} {minigame.Scene}");
				var stateObject = this.Scene.CreateObject();
				this.MinigameState = stateObject.AddComponent<MinigameGameState>();
				stateObject.NetworkSpawn();
				Log.Info($"Created {nameof(MinigameGameState)}");
			}
			else
			{
				Log.Error($"Failed to load {minigame.SceneType} {minigame.Scene}!");
			}
		}

		private bool OnRoundEnded_Validate()
		{
			return true;
		}
	}
}
