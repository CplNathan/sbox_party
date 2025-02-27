using SandboxParty.Events;
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

		[Sync( Flags = SyncFlags.FromHost )] public BoardComponent CurrentTile { get; set; }

		[Sync( Flags = SyncFlags.FromHost )] public NetList<BoardComponent> CurrentPath { get; set; } = [];

		public bool TargetReached { get; set; } = true;

		public bool DestinationReached { get; set; } = true;

		private List<BoardComponent> AllTiles { get => [.. Scene.Components.GetAll<BoardComponent>()]; }

		public void MoveForward( int steps )
		{
			CurrentPath.Clear();
			GetMovement( steps ).ForEach( x => CurrentPath.Add( x ) );
		}

		private List<BoardComponent> GetMovement( int steps = 1 )
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
			if ( TargetReached && targetTile?.IsValid == true )
			{
				var targetPosition = Scene?.NavMesh?.GetClosestPoint( targetTile.WorldPosition );
				NavigationAgent.MoveTo( targetPosition.Value );

				CurrentPath.RemoveAt( 0 );
				CurrentTile = targetTile;
			}

			var isStationary = NavigationAgent.Velocity.IsNearlyZero();
			NavigationArea.IsBlocker = isStationary;
			DesiredLocation = isStationary ? DesiredLocation : NavigationAgent.AgentPosition;
			TargetReached = NavigationAgent.TargetPosition?.IsNearlyZero() == true || (NavigationAgent.TargetPosition.HasValue && NavigationAgent.AgentPosition.Distance( NavigationAgent.TargetPosition.Value ) <= 32);
			DestinationReached = TargetReached && CurrentPath.Count == 0;

			if ( DestinationReached && NavigationAgent.TargetPosition?.IsNearlyZero() == false )
			{
				NavigationAgent.Stop();
				IBoardCharacterEvent.PostToGameObject( GameObject, x => x.OnDestinationReached() );
			}
		}

		private void HandleRotation()
		{
			Vector3 vector = NavigationAgent.GetLookAhead( 30f ) - base.WorldPosition;
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

			CurrentTile ??= AllTiles.First();

			HandleMovement();
			HandleRotation();
		}
	}
}
