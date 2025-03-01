namespace SandboxParty.Events
{
	public interface IBoardComponentEvent : ISceneEvent<IBoardComponentEvent>
	{
		public void OnComponentOccupied();
	}
}
