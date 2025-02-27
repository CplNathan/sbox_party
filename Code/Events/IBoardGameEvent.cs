using SandboxParty.Components.Board.Character;

namespace SandboxParty.Events
{
    public interface IBoardGameEvent : ISceneEvent<IBoardGameEvent>
	{
		void OnTurnEnded(BoardCharacter character);
    }
}
