namespace SandboxParty.Events.Board
{
    public interface IBoardRoundEvent : ISceneEvent<IBoardRoundEvent>
    {
		void OnRoundEnded();
	}
}
