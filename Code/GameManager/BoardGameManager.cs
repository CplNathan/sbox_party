using SandboxParty.Components.Board;
using System.Threading.Tasks;

namespace SandboxParty.GameManager
{
	public sealed class BoardGameManager : GameObjectSystem<BoardGameManager>, ISceneStartup
	{
		public BoardGameState BoardGameState { get => _boardGameState ??= Scene.GetComponentInChildren<BoardNetworkHelper>().GameState; }

		private BoardGameState _boardGameState;

		private readonly SceneLoadOptions _lobbyOptions;

		private readonly SceneLoadOptions _boardOptions;

		public BoardGameManager( Scene scene ) : base( scene )
		{
			_lobbyOptions = new SceneLoadOptions
			{
				IsAdditive = false,
				DeleteEverything = false,
			};
			_lobbyOptions.SetScene( "scenes/start.scene" );

			_boardOptions = new SceneLoadOptions
			{
				IsAdditive = false,
				DeleteEverything = false,
			};
			_boardOptions.SetScene( "scenes/minimal.scene" );
		}

		void ISceneStartup.OnHostInitialize()
		{
			Scene.Load( _lobbyOptions );

			if ( !Networking.IsActive )
			{
				LoadingScreen.Title = "Creating Lobby";
				Networking.CreateLobby( new() );
			}

			GameTask.RunInThreadAsync( async () =>
			{
				await GameTask.DelayRealtimeSeconds( 1 );
				await GameTask.MainThread();

				Scene.Load( _boardOptions );
			} );
		}

		void ISceneStartup.OnClientInitialize()
		{
			//NetworkSp
			// throw new System.NotImplementedException();
			//GameState = new GameResource()
		}
	}
}
