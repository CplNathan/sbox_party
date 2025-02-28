﻿using SandboxParty.Events;
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

		private BoardComponent GoalTile { get; set; }

		private BoardComponent CurrentTile { get; set; }

		private List<BoardComponent> CurrentPath { get; set; } = [];

		private bool IsOurTurn { get => BoardGameManager.Current.BoardGameState?.CurrentTurn.GameObject == GameObject; }

		protected override void OnStart()
		{
			base.OnStart();

			NavigationAgent.SetAgentPosition( GameObject.WorldPosition );

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

		private void HandleMovement()
		{
			var previousTargetReached = NavigationAgent.TargetPosition?.IsNearlyZero() == true || NavigationAgent.AgentPosition.Distance( NavigationAgent.TargetPosition ?? Vector3.Zero ) <= 32;
			var targetTile = CurrentPath.FirstOrDefault();
			if ( previousTargetReached && targetTile?.IsValid == true )
			{
				var targetPosition = Scene?.NavMesh?.GetClosestPoint( targetTile.WorldPosition );
				NavigationAgent.MoveTo( targetPosition.Value );

				CurrentPath.RemoveAt( 0 );
				CurrentTile = targetTile;
			}

			var isStationary = NavigationAgent.Velocity.IsNearlyZero();
			var isOurTurn = BoardGameManager.Current.BoardGameState?.CurrentTurn.GameObject == GameObject;
			NavigationArea.IsBlocker = isStationary && !IsOurTurn;
			DesiredLocation = NavigationAgent.AgentPosition;

			var destinationReached = previousTargetReached && CurrentPath.Count == 0 && GoalTile?.IsValid == true && NavigationAgent.AgentPosition.Distance( GoalTile.WorldPosition ) <= 32;
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

		public void MoveForward( int steps )
		{
			CurrentPath = GetMovement( steps );
			GoalTile = CurrentPath.Last();
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
	}
}
