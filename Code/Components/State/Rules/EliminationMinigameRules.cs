using SandboxParty.Components.Character.Minigame;
using SandboxParty.Components.State.Rules;
using SandboxParty.Events.Minigame;
using System;

namespace SandboxParty.Components.State.Minigame
{
	public class EliminationMinigameRules : BaseMinigameRules<EliminationMinigameRules>, IMinigameRoundEvent, IMinigameRules
	{
		public void OnEliminated(MinigameCharacter character)
		{
			IMinigameRoundEvent.Post(x => x.OnRoundEnded());
		}

		void IMinigameRoundEvent.OnRoundEnded()
		{
			throw new NotImplementedException();
		}
	}
}
