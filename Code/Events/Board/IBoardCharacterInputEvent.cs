// <copyright file="IBoardCharacterEvent.cs" company="Nathan Ford">
// Copyright (c) Nathan Ford. All rights reserved.
// </copyright>


// <copyright file="IBoardCharacterEvent.cs" company="Nathan Ford">
// Copyright (c) Nathan Ford. All rights reserved.
// </copyright>

using SandboxParty.Components.World.Board;

namespace SandboxParty.Events.Board
{
	public interface IBoardCharacterInputEvent : ISceneEvent<IBoardCharacterInputEvent>
	{
		void OnDiceRoll();

		void OnDirectionDecided(BoardComponent direction);

		void OnEndTurn();
	}
}
