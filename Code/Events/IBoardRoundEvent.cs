namespace SandboxParty.Events
{
    public interface IBoardRoundEvent : ISceneEvent<IBoardRoundEvent>
    {
		void OnRoundEnded();
	}
}
