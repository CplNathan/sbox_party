using SandboxParty.Components.Board.Character;

namespace SandboxParty.Events
{
    public interface IBoardGameEvent : ISceneEvent<IBoardGameEvent>
	{
		void OnPlayerJoined( Connection channel, GameObject playerPrefab, Transform startLocation );

		void OnPlayerLeft( Connection channel );

		void OnDestinationReached( BoardCharacter character );

		void OnTurnEnded(BoardCharacter character);
    }
}
