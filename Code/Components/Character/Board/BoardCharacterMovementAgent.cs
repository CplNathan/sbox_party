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

		private BoardComponent CurrentTarget { get; set; }

		private BoardComponent CurrentTile { get; set; }

		private List<BoardComponent> CurrentPath { get; set; } = [];

		protected override void OnStart()
		{
			base.OnStart();

			NavigationAgent.SetAgentPosition( GameObject.WorldPosition );
			NavigationAgent.MoveTo( GameObject.WorldPosition );

			CurrentTile = Scene.Components.GetAll<BoardComponent>().First();
		}

		protected override void OnUpdate()
		{
			base.OnUpdate();

			Velocity = NavigationAgent.Velocity;
			WishVelocity = NavigationAgent.WishVelocity;

			HandleMovement();
			HandleRotation();
		}

		public void MoveForward( int steps )
		{
			CurrentPath = GetPath( steps );
			CurrentTarget = CurrentPath.Last();
		}

		private List<BoardComponent> GetPath( int steps = 1 )
		{
			List<BoardComponent> tilePath = [];

			for ( int i = 0; i < steps; i++ )
			{
				var nextComponent = CurrentTile.NextComponent.ElementAt( Random.Shared.Next( 0, CurrentTile.NextComponent.Length ) );
				CurrentTile = nextComponent.Components.Get<BoardComponent>();
				tilePath.Add( CurrentTile );
			}

			return tilePath;
		}

		private void HandleMovement()
		{
			var targetTile = CurrentPath.FirstOrDefault();
			var hasNoNavigationPath = NavigationAgent.TargetPosition?.IsNearlyZero() == true;
			var hasReachedTile = hasNoNavigationPath || NavigationAgent.AgentPosition.Distance( NavigationAgent.TargetPosition.Value ) <= 32;
			var hasReachedTarget = hasReachedTile && CurrentPath.Count == 0 && NavigationAgent.AgentPosition.Distance( CurrentTarget?.WorldPosition ?? Vector3.Zero ) <= 32;
			if ( hasReachedTile && targetTile?.IsValid == true )
			{
				var targetPosition = Scene.NavMesh?.GetClosestPoint( targetTile.WorldPosition );
				NavigationAgent.MoveTo( targetPosition.Value );

				CurrentPath.RemoveAt( 0 );
				CurrentTile = targetTile;
			}
			else if ( hasReachedTarget && !hasNoNavigationPath )
			{
				IBoardCharacterEvent.PostToGameObject( GameObject, x => x.OnDestinationReached() );
			}

			var isStationary = NavigationAgent.Velocity.IsNearlyZero();
			var isOurTurn = GameManager.Current.BoardState?.CurrentTurn.GameObject == GameObject;
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
	}
}
