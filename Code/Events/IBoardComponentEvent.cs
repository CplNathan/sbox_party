// <copyright file="IBoardComponentEvent.cs" company="Nathan Ford">
// Copyright (c) Nathan Ford. All rights reserved.
// </copyright>

namespace SandboxParty.Events
{
	public interface IBoardComponentEvent : ISceneEvent<IBoardComponentEvent>
	{
		public void OnComponentOccupied();
	}
}
