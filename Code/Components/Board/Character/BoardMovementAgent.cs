using SandboxParty.Events;
using SandboxParty.GameManager;
using System;

namespace SandboxParty.Components.Board.Character
{
	public class BoardMovementAgent : Component
	{
		[RequireComponent] public NavMeshAgent NavigationAgent { get; set; }

		[RequireComponent] public NavMeshArea NavigationArea { get; set; }

		[Sync( Flags = SyncFlags.FromHost | SyncFlags.Interpolate )] public Vector3 Velocity { get; set; }

		[Sync( Flags = SyncFlags.FromHost | SyncFlags.Interpolate )] public Vector3 WishVelocity { get; set; }

		[Sync( Flags = SyncFlags.FromHost | SyncFlags.Interpolate )] public Vector3 DesiredLocation { get; set; }

		[Sync( Flags = SyncFlags.FromHost | SyncFlags.Interpolate )] public Rotation DesiredRotation { get; set; }

		private BoardComponent goalTile;

		private BoardComponent currentTile;

		private List<BoardComponent> currentPath = [];

		private bool IsOurTurn { get => BoardGameManager.Current.BoardGameState?.CurrentTurn.GameObject == GameObject; }

		protected override void OnStart()
		{
			base.OnStart();

			NavigationAgent.SetAgentPosition( GameObject.WorldPosition );

			currentTile = Scene.Components.GetAll<BoardComponent>().First();
		}

		public void MoveForward( int steps )
		{
			currentPath = GetMovement( steps );
			goalTile = currentPath.Last();
		}

		private List<BoardComponent> GetMovement( int steps = 1 )
		{
			List<BoardComponent> tilePath = [];

			for ( int i = 0; i < steps; i++ )
			{
				var nextComponent = currentTile.NextComponent.ElementAt( Random.Shared.Next( 0, currentTile.NextComponent.Length ) );
				currentTile = nextComponent.Components.Get<BoardComponent>();
				tilePath.Add( currentTile );
			}

			return tilePath;
		}

		private void HandleMovement()
		{
			var targetReached = NavigationAgent.TargetPosition?.IsNearlyZero() == true || NavigationAgent.AgentPosition.Distance( NavigationAgent.TargetPosition ?? Vector3.Zero ) <= 32;
			var targetTile = currentPath.FirstOrDefault();
			if ( targetReached && targetTile?.IsValid == true )
			{
				var targetPosition = Scene?.NavMesh?.GetClosestPoint( targetTile.WorldPosition );
				NavigationAgent.MoveTo( targetPosition.Value );

				currentPath.RemoveAt( 0 );
				currentTile = targetTile;
			}

			var isStationary = NavigationAgent.Velocity.IsNearlyZero();
			NavigationArea.IsBlocker = isStationary && !IsOurTurn;
			DesiredLocation = NavigationAgent.AgentPosition;

			var destinationReached = targetReached && currentPath.Count == 0 && goalTile?.IsValid == true && NavigationAgent.AgentPosition.Distance( goalTile.WorldPosition ) <= 32;
			if ( destinationReached && NavigationAgent.TargetPosition?.IsNearlyZero() == false )
			{
				IBoardCharacterEvent.PostToGameObject( GameObject, x => x.OnDestinationReached() );
			}
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

		protected override void OnUpdate()
		{
			base.OnUpdate();

			Velocity = NavigationAgent.Velocity;
			WishVelocity = NavigationAgent.WishVelocity;

			HandleMovement();
			HandleRotation();
		}
	}
}
