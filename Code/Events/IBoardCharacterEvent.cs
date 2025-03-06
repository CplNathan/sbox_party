// <copyright file="IBoardCharacterEvent.cs" company="Nathan Ford">
// Copyright (c) Nathan Ford. All rights reserved.
// </copyright>

namespace SandboxParty.Events
{
	public interface IBoardCharacterEvent : ISceneEvent<IBoardCharacterEvent>
	{
		void OnDestinationReached();
	}
}
