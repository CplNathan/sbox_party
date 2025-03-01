using SandboxParty.Components.Character.Board;

namespace SandboxParty.Events
{
	public interface IBoardCharacterEvent : ISceneEvent<IBoardCharacterEvent>
	{
		void OnDestinationReached();
	}
}
