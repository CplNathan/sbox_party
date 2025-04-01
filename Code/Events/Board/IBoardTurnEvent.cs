// <copyright file="IBoardEvent.cs" company="Nathan Ford">
// Copyright (c) Nathan Ford. All rights reserved.
// </copyright>


// <copyright file="IBoardEvent.cs" company="Nathan Ford">
// Copyright (c) Nathan Ford. All rights reserved.
// </copyright>

using SandboxParty.Components.Character.Board;

namespace SandboxParty.Events.Board
{
	public interface IBoardTurnEvent : ISceneEvent<IBoardTurnEvent>
	{
		void OnTurnStarted(BoardCharacter character);

		void OnDestinationReached(BoardCharacter character);

		void OnTurnEnded(BoardCharacter character);
	}
}
