using SandboxParty.Components.Board;
using SandboxParty.Components.State;
using SandboxParty.Events;
using SandboxParty.Managers;
using System;
using GameManager = SandboxParty.Managers.GameManager;

namespace SandboxParty.Components.Character.Board
{


	public class BoardCharacterMovementAgent : Component
	{
		[RequireComponent] public NavMeshAgent NavigationAgent { get; set; }

		[Sync( Flags = SyncFlags.FromHost )] public Vector3 Velocity { get; set; }

		[Sync( Flags = SyncFlags.FromHost )] public Vector3 WishVelocity { get; set; }

		[Sync( Flags = SyncFlags.FromHost | SyncFlags.Interpolate )] public Vector3 DesiredLocation { get; set; }

		[Sync( Flags = SyncFlags.FromHost | SyncFlags.Interpolate )] public Rotation DesiredRotation { get; set; }

		private BoardPathComponent CurrentTile { get; set; }

		public int Steps { get => steps; set => steps += value; }

		private int steps = 0;

		protected override void OnStart()
		{
			base.OnStart();

			NavigationAgent.SetAgentPosition( GameObject.WorldPosition );
			NavigationAgent.MoveTo( GameObject.WorldPosition );
			NavigationAgent.Stop();

			CurrentTile = new BoardPathComponent( Scene.Components.GetAll<BoardComponent>().First() );
		}

		protected override void OnUpdate()
		{
			base.OnUpdate();

			Velocity = NavigationAgent.Velocity;
			WishVelocity = NavigationAgent.WishVelocity;

			DebugRendering();

			BoardComponent requestedComponent = null;
			if ( CurrentTile.SelectionRequired && !CurrentTile.SelectionMade )
			{
				var currentComponent = CurrentTile.Component;
				var closestComponent = CurrentTile.NextComponents.OrderBy( component => component.WorldPosition.Distance( Scene.Camera.ScreenToWorld( Gizmo.CursorPosition ) ) ).First();

				foreach ( var nextComponent in CurrentTile.NextComponents )
				{
					var forwardDirection = Rotation.LookAt( nextComponent.WorldPosition - currentComponent.WorldPosition ).Forward;

					Gizmo.Draw.Color = nextComponent == closestComponent ? Color.Yellow : Color.White;
					Gizmo.Draw.Arrow( currentComponent.WorldPosition + (forwardDirection * 10), currentComponent.WorldPosition + (forwardDirection * 25) );
				}

				if ( Input.Pressed( "attack1" ) )
				{
					requestedComponent = closestComponent;
				}
			}

			// IBoardCharacterEvent.PostToGameObject( GameObject, x => x.OnDestinationReached() );

			if ( Steps > 0 && CurrentTile.Reached( NavigationAgent.AgentPosition ) && CurrentTile.MoveNext( requestedComponent, out BoardPathComponent? nextTile ) )
			{
				CurrentTile = nextTile.Value;
				NavigationAgent.MoveTo( nextTile.Value.Component.WorldPosition );

				Steps = -1;
			}
			else if ( Steps == 0 && CurrentTile.CanSendDestinationNotification( GameObject ) )
			{
				IBoardCharacterEvent.PostToGameObject( GameObject, x => x.OnDestinationReached() );
			}

			HandleMovement();
			HandleRotation();
		}

		private void HandleMovement()
		{
			DesiredLocation = NavigationAgent.AgentPosition;
		}

		private void HandleRotation()
		{
			Vector3 vector = NavigationAgent.GetLookAhead( 30f ) - GameObject.WorldPosition;
			vector.z = 0f;

			if ( vector.Length > 0.1f )
			{
				Rotation rotation = Rotation.LookAt( vector.Normal );
				DesiredRotation = rotation;
			}
		}

		private void DebugRendering()
		{
			RenderPath( CurrentTile, Steps );
			RenderBranch();
		}

		private void RenderBranch()
		{

		}

		private void RenderPath( BoardPathComponent tile, int steps )
		{
			var previousTile = tile;

			for ( int step = 1; step < Steps; step++ )
			{
				var requiresDecision = tile.SelectionRequired && !tile.SelectionMade;
				if ( requiresDecision )
				{
					for ( int branch = 0; branch < previousTile.NextComponents.Length; branch++ )
					{
						Gizmo.Draw.Color = (branch % 2) == 0 ? Color.Red : Color.Blue;
						RenderPath( new BoardPathComponent( previousTile.NextComponents[branch] ), steps - step - 1 );
					}

					return;
				}

				Gizmo.Draw.Text( $"{step}", new Transform( previousTile.Component.WorldPosition, Rotation.LookAt( Scene.Camera.WorldRotation.Forward, Vector3.Up ) ), size: 24 );

				previousTile = new BoardPathComponent( previousTile.NextComponents[0] );
			}
		}
	}
}
