// <copyright file="MinigameGameState.cs" company="Nathan Ford">
// Copyright (c) Nathan Ford. All rights reserved.
// </copyright>

using SandboxParty.Components.Character;
using SandboxParty.Components.Character.Minigame;
using SandboxParty.Components.State.Minigame;
using SandboxParty.Events.Minigame;
using SandboxParty.Resources;

namespace SandboxParty.Components.State
{
	public sealed class MinigameGameState : BaseGameState<MinigameCharacter>, IMinigameRoundEvent
	{
		protected override GameObject PlayerPrefab
			=> SceneResource.GetSceneResource(Scene, SceneResource.Minigames).GetPlayerPrefab();

		protected override Vector3 SpawnLocation
			=> Scene.GetComponentInChildren<SpawnPoint>().WorldPosition;

		void IMinigameRoundEvent.OnRoundEnded()
		{
			throw new System.NotImplementedException();
		}

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
