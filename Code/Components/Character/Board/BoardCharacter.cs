using Sandbox.Citizen;
using SandboxParty.Components.State;
using SandboxParty.Events;
using SandboxParty.Managers;
using GameManager = SandboxParty.Managers.GameManager;

namespace SandboxParty.Components.Character.Board
{
	public class BoardCharacter : BaseCharacter, IBoardCharacterEvent
	{
		[RequireComponent] public BoardCharacterMovementAgent MovementHelper { get; set; }

		[RequireComponent] public CitizenAnimationHelper AnimationHelper { get; set; }

		[RequireComponent] public BoardCharacterDice Dice { get; set; }

		private bool IsOurTurn { get => GameManager.Current.BoardState?.CurrentTurn == this; }

		protected override void OnUpdate()
		{
			base.OnUpdate();

			UpdateAnimations();
			UpdateLocation();
			UpdateRotation();

			if ( IsProxy )
				return;

			Gizmo.Draw.ScreenText( $"You rolled {Dice.LastRoll}", new Vector2( 50, 50 ) );

			if ( Input.Pressed( "jump" ) && RollDice_Validate() )
				RollDice();
		}

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
			GameObject.WorldRotation = Rotation.Slerp( GameObject.WorldRotation, MovementHelper.DesiredRotation, Time.Delta * 3f );
		}

		void IBoardCharacterEvent.OnDestinationReached()
		{
			IBoardEvent.Post( x => x.OnDestinationReached( this ) );
		}

		[Rpc.Host( Flags = NetFlags.OwnerOnly )]
		public void RollDice()
		{
			if ( !RollDice_Validate() )
				return;

			Vector3 forwardVector = AnimationHelper.Target.WorldRotation.Forward;
			Vector3 upVector = Vector3.Up;
			Vector3 vector = GameObject.WorldPosition + forwardVector * 50 + upVector * 25;

			Dice.RollDice( vector ).ContinueWith(newRoll =>
			{
				MovementHelper.Steps = newRoll.Result;
			} );
		}

		[Rpc.Host( Flags = NetFlags.OwnerOnly )]
		public void EndTurn()
		{
			if ( !EndTurn_Validate() )
				return;

			IBoardEvent.Post( x => x.OnTurnEnded( this ) );
		}

		public bool RollDice_Validate()
		{
			return IsOurTurn;
		}

		public bool EndTurn_Validate()
		{
			return IsOurTurn;
		}
	}
}
