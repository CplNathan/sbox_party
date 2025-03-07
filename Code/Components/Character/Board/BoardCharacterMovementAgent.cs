// <copyright file="BoardCharacterMovementAgent.cs" company="Nathan Ford">
// Copyright (c) Nathan Ford. All rights reserved.
// </copyright>

using SandboxParty.Components.World.Board;
using SandboxParty.Events;

namespace SandboxParty.Components.Character.Board
{
	public class BoardCharacterMovementAgent : Component
	{
		private int steps = 0;

		public int Steps { get => this.steps; set => this.steps += value; }

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

		[Sync(Flags = SyncFlags.FromHost)]
		public BoardPathComponent CurrentTile { get; set; }

		[Sync]
		public BoardComponent RequestedComponent { get; set; }

		protected override void OnStart()
		{
			base.OnStart();

			this.CurrentTile = new BoardPathComponent(this.Scene.GetComponentsInChildren<BoardComponent>().First());
			this.NavigationAgent.SetAgentPosition(this.CurrentTile.Component.WorldPosition);
			this.NavigationAgent.MoveTo(this.CurrentTile.Component.WorldPosition);
		}

		protected override void OnUpdate()
		{
			base.OnUpdate();

			if (Networking.IsHost)
			{
				this.HandleNavigation(RequestedComponent);
			}

			this.DebugRendering();
			this.HandleMovement();
			this.HandleRotation();

			if (this.Network.IsProxy)
			{
				return;
			}

			if (Input.Pressed("attack1"))
			{
				var closestComponent = this.CurrentTile.NextComponents.OrderBy(component => component.WorldPosition.Distance(this.Scene.Camera.ScreenToWorld(Gizmo.CursorPosition))).First();
				RequestedComponent = closestComponent;
			}
		}

		private void HandleNavigation(BoardComponent requestedComponent = null)
		{
			var destinationReached = this.CurrentTile.Reached(this.NavigationAgent.AgentPosition, out bool justReached);
			if (this.Steps > 0 && destinationReached && this.CurrentTile.MoveNext(requestedComponent, out BoardPathComponent nextTile))
			{
				this.CurrentTile = nextTile;
				this.NavigationAgent.MoveTo(this.CurrentTile.Component.WorldPosition);

				this.Steps = -1;
			}
			else if (this.Steps == 0 && destinationReached && justReached)
			{
				IBoardCharacterEvent.PostToGameObject(this.GameObject, x => x.OnDestinationReached());
				// this.NavigationAgent.Stop();
			}
		}

		private void HandleMovement()
		{
			this.Velocity = this.NavigationAgent.Velocity;
			this.WishVelocity = this.NavigationAgent.WishVelocity;
			this.DesiredLocation = this.NavigationAgent.AgentPosition;
		}

		private void HandleRotation()
		{
			Vector3 vector = this.NavigationAgent.GetLookAhead(30f) - this.GameObject.WorldPosition;
			vector.z = 0f;

			if (vector.Length > 0.1f)
			{
				Rotation rotation = Rotation.LookAt(vector.Normal);
				this.DesiredRotation = rotation;
			}
		}

		private void DebugRendering()
		{
			this.RenderPath(this.CurrentTile, this.Steps);
			this.RenderBranch();
		}

		private void RenderBranch()
		{
			var currentComponent = this.CurrentTile.Component;

			if (this.CurrentTile.SelectionRequired && !this.CurrentTile.SelectionMade)
			{
				foreach (var nextComponent in this.CurrentTile.NextComponents)
				{
					var forwardDirection = Rotation.LookAt(nextComponent.WorldPosition - currentComponent.WorldPosition).Forward;

					Gizmo.Draw.Color = nextComponent == RequestedComponent ? Color.Yellow : Color.White;
					Gizmo.Draw.Arrow(currentComponent.WorldPosition + (forwardDirection * 10), currentComponent.WorldPosition + (forwardDirection * 25));
				}
			}
		}

		private void RenderPath(BoardPathComponent tile, int steps)
		{
			var previousTile = tile;

			for (int step = 1; step <= this.Steps; step++)
			{
				var requiresDecision = tile.SelectionRequired && !tile.SelectionMade;
				if (requiresDecision)
				{
					for (int branch = 0; branch < previousTile.NextComponents.Count; branch++)
					{
						Gizmo.Draw.Color = (branch % 2) == 0 ? Color.Red : Color.Blue;
						this.RenderPath(new BoardPathComponent(previousTile.NextComponents[branch]), steps - step - 1);
					}

					return;
				}

				Gizmo.Draw.Text($"{step}", new Transform(previousTile.Component.WorldPosition, Rotation.LookAt(this.Scene.Camera.WorldRotation.Forward, Vector3.Up)), size: 24);

				previousTile = new BoardPathComponent(previousTile.NextComponents[0]);
			}
		}
	}
}
