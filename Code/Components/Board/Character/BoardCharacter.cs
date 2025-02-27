using Sandbox.Citizen;
using SandboxParty.Events;
using SandboxParty.GameManager;
using System;
using System.Numerics;
using System.Threading.Tasks;

namespace SandboxParty.Components.Board.Character
{
	public class BoardCharacter : Component, IBoardCharacterEvent
	{
		[RequireComponent] public BoardMovementAgent MovementHelper { get; set; }

		[RequireComponent] public CitizenAnimationHelper AnimationHelper { get; set; }

		[RequireComponent] public BoardCharacterDice Dice { get; set; }

		[Sync( Flags = SyncFlags.FromHost )] public int LastRoll { get; set; } = 0;

		private bool IsOurTurn { get => BoardGameManager.Current.BoardGameState?.CurrentTurn == this; }

		private void UpdateAnimations()
		{
			AnimationHelper.WithVelocity( MovementHelper.Velocity );
			AnimationHelper.WithWishVelocity( MovementHelper.WishVelocity );
			AnimationHelper.Sitting = IsOurTurn ? CitizenAnimationHelper.SittingStyle.None : CitizenAnimationHelper.SittingStyle.Floor;
		}

		private void UpdateLocation()
		{
			GameObject.WorldPosition = MovementHelper.DesiredLocation;
		}

		private void UpdateRotation()
		{
			AnimationHelper.Target.WorldRotation = Rotation.Slerp( AnimationHelper.Target.WorldRotation, MovementHelper.DesiredRotation, Time.Delta * 3.0f );
		}

		protected override void OnUpdate()
		{
			base.OnUpdate();

			var camera = GameObject.GetComponentInChildren<CameraComponent>();
			if ( camera?.IsValid == true )
				camera.Priority = Network.IsOwner ? 2 : 1;

			UpdateAnimations();
			UpdateLocation();
			UpdateRotation();

			if ( IsProxy )
				return;

			Gizmo.Draw.ScreenText( $"You rolled {LastRoll}", new Vector2( 50, 50 ) );

			if ( Input.Pressed( "jump" ) && RollDiceRPC_Validate() )
				RollDiceRPC();


			// AnimationHelper.Sitting = CitizenAnimationHelper.SittingStyle.Floor
			// NavigationAgent.MoveTo( Vector3.Random );
		}

		[Rpc.Host( Flags = NetFlags.OwnerOnly )]
		public async void RollDiceRPC()
		{
			if ( !RollDiceRPC_Validate() )
				return;

			Vector3 forwardVector = AnimationHelper.Target.WorldRotation.Forward;
			Vector3 upVector = Vector3.Up;
			Vector3 vector = GameObject.WorldPosition + forwardVector * 50 + upVector * 25;

			LastRoll = await Dice.RollDice( vector );
			MovementHelper.MoveForward( LastRoll );
		}

		public bool RollDiceRPC_Validate()
		{
			return IsOurTurn;
		}

		public void OnDestinationReached()
		{
			IBoardGameEvent.Post( x => x.OnTurnEnded( this ) );
		}
	}
}
