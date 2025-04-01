// <copyright file="BoardCharacterMovementAgent.cs" company="Nathan Ford">
// Copyright (c) Nathan Ford. All rights reserved.
// </copyright>

using SandboxParty.Components.World.Board;
using SandboxParty.Events.Board;
using SandboxParty.Structs;

namespace SandboxParty.Components.Character.Board
{
	public class BoardCharacterMovementAgent : Component, Component.INetworkSpawn
	{
		public int Steps { get; set; }

		[RequireComponent]
		public NavMeshAgent NavigationAgent { get; set; }

		[Sync(Flags = SyncFlags.FromHost)]
		public Vector3 Velocity { get; set; }

		[Sync(Flags = SyncFlags.FromHost)]
		public Vector3 WishVelocity { get; set; }

		[Sync(Flags = SyncFlags.FromHost | SyncFlags.Interpolate)]
		public Vector3 DesiredLocation { get; set; }

		[Sync(Flags = SyncFlags.FromHost | SyncFlags.Interpolate)]
		public Rotation DesiredRotation { get; set; }

		[Sync(Flags = SyncFlags.FromHost | SyncFlags.Query)]
		public BoardTile CurrentTile { get => _currentTile; set => _currentTile = value; }

		[Sync]
		public BoardComponent RequestedComponent { get; set; }

		private BoardTile _currentTile;

		protected override void OnUpdate()
		{
			base.OnUpdate();

			if (this.IsValid && Networking.IsHost)
			{
				HandleNavigation();
				HandleMovement();
				HandleRotation();
			}
		}

		void INetworkSpawn.OnNetworkSpawn(Connection owner)
		{
			_currentTile = new BoardTile(Scene.GetComponentsInChildren<BoardComponent>().First());
			NavigationAgent.SetAgentPosition(_currentTile.Component.WorldPosition);
			NavigationAgent.MoveTo(_currentTile.Component.WorldPosition);
		}

		private void HandleNavigation()
		{
			var destinationReached = _currentTile.Reached(NavigationAgent.AgentPosition, out bool justReached);
			if (Steps > 0 && destinationReached && _currentTile.MoveNext(RequestedComponent, out BoardTile? nextTile))
			{
				_currentTile = nextTile.Value;
				NavigationAgent.MoveTo(_currentTile.Component.WorldPosition);

				Steps--;
			}
			else if (Steps == 0 && destinationReached && justReached)
			{
				IBoardCharacterEvent.PostToGameObject(GameObject, x => x.OnDestinationReached());
			}
		}

		private void HandleMovement()
		{
			Velocity = NavigationAgent.Velocity;
			WishVelocity = NavigationAgent.WishVelocity;
			DesiredLocation = NavigationAgent.AgentPosition;
		}

		private void HandleRotation()
		{
			Vector3 vector = NavigationAgent.GetLookAhead(30f) - GameObject.WorldPosition;
			vector.z = 0f;

			if (vector.Length > 0.1f)
			{
				Rotation rotation = Rotation.LookAt(vector.Normal);
				DesiredRotation = rotation;
			}
		}
	}
}
