// <copyright file="MinigameGameState.cs" company="Nathan Ford">
// Copyright (c) Nathan Ford. All rights reserved.
// </copyright>

using SandboxParty.Components.Character;
using SandboxParty.Resources;

namespace SandboxParty.Components.State
{
	public sealed class MinigameGameState : BaseGameState<BaseCharacter>
	{
		protected override GameObject GetPlayerPrefab()
			=> SceneResource.GetSceneResource(this.Scene, SceneResource.Minigames).GetPlayerPrefab();

		protected override void OnConnected(Connection channel)
		{
			throw new System.NotImplementedException();
		}

		protected override void OnDisconnected(Connection channel)
		{
			throw new System.NotImplementedException();
		}
	}
}
