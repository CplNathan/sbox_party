using Sandbox;
using SandboxParty.Components.Character;
using SandboxParty.Resources;

namespace SandboxParty.Components.State
{
	public sealed class MinigameGameState : BaseGameState<BaseCharacter>
	{
		protected override GameObject GetPlayerPrefab()
			=> SceneResource.GetSceneResource( Scene, SceneResource.Minigames ).GetPlayerPrefab();
	}
}
