namespace SandboxParty.Events
{
    public interface IBoardCharacterEvent : ISceneEvent<IBoardCharacterEvent>
    {
		void OnDestinationReached();
    }
}
