using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SandboxParty.Events
{
    public interface IBoardCharacterEvent : ISceneEvent<IBoardCharacterEvent>
    {
		void OnDestinationReached();
    }
}
