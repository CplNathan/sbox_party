namespace SandboxParty.Components.State.Rules
{
	public interface IMinigameRules
	{
		public void AddComponent(GameObject gameObject);
	}

	public abstract class BaseMinigameRules<T> : Component, IMinigameRules
		where T : Component, new()
	{
		public void AddComponent(GameObject gameObject)
		{
			gameObject.AddComponent<T>();
		}
	}
}
