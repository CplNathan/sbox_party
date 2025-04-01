using SandboxParty.Components.Character.Minigame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SandboxParty.Events.Minigame
{
    public interface IMinigameRoundEvent : ISceneEvent<IMinigameRoundEvent>
	{
		void OnRoundEnded();
	}
}
