// <copyright file="IBoardEvent.cs" company="Nathan Ford">
// Copyright (c) Nathan Ford. All rights reserved.
// </copyright>

using SandboxParty.Components.Character.Board;

namespace SandboxParty.Events
{
	public interface IBoardEvent : ISceneEvent<IBoardEvent>
	{
		void OnTurnStarted(BoardCharacter character);

		void OnDestinationReached(BoardCharacter character);

		void OnTurnEnded(BoardCharacter character);
	}
}
