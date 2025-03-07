// <copyright file="BoardCharacterMovementAgent.cs" company="Nathan Ford">
// Copyright (c) Nathan Ford. All rights reserved.
// </copyright>

using SandboxParty.Components.World.Board;
using SandboxParty.Events;
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

			if (Networking.IsHost)
			{
				HandleNavigation(RequestedComponent);
			}

			DebugRendering();
			HandleMovement();
			HandleRotation();

			if (Network.IsProxy)
			{
				return;
			}

			if (Input.Pressed("attack1"))
			{
				var closestComponent = _currentTile.NextComponents.OrderBy(component => component.WorldPosition.Distance(Scene.Camera.ScreenToWorld(Gizmo.CursorPosition))).First();
				RequestedComponent = closestComponent;
			}
		}

		void INetworkSpawn.OnNetworkSpawn(Connection owner)
		{
			_currentTile = new BoardTile(Scene.GetComponentsInChildren<BoardComponent>().First());
			NavigationAgent.SetAgentPosition(_currentTile.Component.WorldPosition);
			NavigationAgent.MoveTo(_currentTile.Component.WorldPosition);
		}

		private void HandleNavigation(BoardComponent requestedComponent = null)
		{
			var destinationReached = _currentTile.Reached(NavigationAgent.AgentPosition, out bool justReached);
			if (Steps > 0 && destinationReached && _currentTile.MoveNext(requestedComponent, out BoardTile? nextTile))
			{
				_currentTile = nextTile.Value;
				NavigationAgent.MoveTo(_currentTile.Component.WorldPosition);

				Steps--;
			}
			else if (Steps == 0 && destinationReached && justReached)
			{
				IBoardCharacterEvent.PostToGameObject(GameObject, x => x.OnDestinationReached());
				// NavigationAgent.Stop();
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

		private void DebugRendering()
		{
			RenderPath(_currentTile, Steps);
			RenderBranch();
		}

		private void RenderBranch()
		{
			var currentComponent = _currentTile.Component;

			if (_currentTile.SelectionRequired && !_currentTile.SelectionMade)
			{
				foreach (var nextComponent in _currentTile.NextComponents)
				{
					var forwardDirection = Rotation.LookAt(nextComponent.WorldPosition - currentComponent.WorldPosition).Forward;

					Gizmo.Draw.Color = nextComponent == RequestedComponent ? Color.Yellow : Color.White;
					Gizmo.Draw.Arrow(currentComponent.WorldPosition + (forwardDirection * 10), currentComponent.WorldPosition + (forwardDirection * 25));
				}
			}
		}

		private void RenderPath(BoardTile tile, int steps)
		{
			var previousTile = tile;

			for (int step = 1; step <= Steps; step++)
			{
				var requiresDecision = tile.SelectionRequired && !tile.SelectionMade;
				if (requiresDecision)
				{
					for (int branch = 0; branch < previousTile.NextComponents.Count; branch++)
					{
						Gizmo.Draw.Color = (branch % 2) == 0 ? Color.Red : Color.Blue;
						RenderPath(new BoardTile(previousTile.NextComponents[branch]), steps - step - 1);
					}

					return;
				}

				Gizmo.Draw.Text($"{step}", new Transform(previousTile.Component.WorldPosition, Rotation.LookAt(Scene.Camera.WorldRotation.Forward, Vector3.Up)), size: 24);

				previousTile = new BoardTile(previousTile.NextComponents[0]);
			}
		}
	}
}
