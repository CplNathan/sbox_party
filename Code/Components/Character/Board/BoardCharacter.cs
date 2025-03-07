// <copyright file="BoardCharacter.cs" company="Nathan Ford">
// Copyright (c) Nathan Ford. All rights reserved.
// </copyright>

using Sandbox.Citizen;
using SandboxParty.Events;
using GameManager = SandboxParty.Managers.GameManager;

namespace SandboxParty.Components.Character.Board
{
	public class BoardCharacter : BaseCharacter, IBoardCharacterEvent
	{
		[RequireComponent]
		public BoardCharacterMovementAgent MovementHelper { get; set; }

		[RequireComponent]
		public CitizenAnimationHelper AnimationHelper { get; set; }

		[RequireComponent]
		public BoardCharacterDice Dice { get; set; }

		private bool IsOurTurn { get => GameManager.Current.BoardState?.CurrentTurn == this; }

		void IBoardCharacterEvent.OnDestinationReached()
		{
			IBoardTurnEvent.Post(x => x.OnDestinationReached(this));
		}

		[Rpc.Host(Flags = NetFlags.OwnerOnly)]
		public void RollDice()
		{
			if (!this.RollDice_Validate())
			{
				return;
			}

			Vector3 forwardVector = this.AnimationHelper.Target.WorldRotation.Forward;
			Vector3 upVector = Vector3.Up;
			Vector3 vector = this.GameObject.WorldPosition + (forwardVector * 50) + (upVector * 25);

			this.Dice.RollDice(vector).ContinueWith(newRoll =>
			{
				this.MovementHelper.Steps = newRoll.Result;
			});
		}

		[Rpc.Host(Flags = NetFlags.OwnerOnly)]
		public void EndTurn()
		{
			if (!this.EndTurn_Validate())
			{
				return;
			}

			IBoardTurnEvent.Post(x => x.OnTurnEnded(this));
		}

		public bool RollDice_Validate()
		{
			return this.IsOurTurn;
		}

		public bool EndTurn_Validate()
		{
			return this.IsOurTurn;
		}

		protected override void OnUpdate()
		{
			base.OnUpdate();

			this.UpdateAnimations();
			this.UpdateLocation();
			this.UpdateRotation();

			if (this.IsProxy)
			{
				return;
			}

			Gizmo.Draw.ScreenText($"You rolled {this.Dice.LastRoll}", new Vector2(50, 50));

			if (Input.Pressed("jump") && this.RollDice_Validate())
			{
				this.RollDice();
			}
		}

		private void UpdateAnimations()
		{
			this.AnimationHelper.WithVelocity(this.MovementHelper.Velocity);
			this.AnimationHelper.WithWishVelocity(this.MovementHelper.WishVelocity);
			this.AnimationHelper.Sitting = this.IsOurTurn ? CitizenAnimationHelper.SittingStyle.None : CitizenAnimationHelper.SittingStyle.Floor;
		}

		private void UpdateLocation()
		{
			this.GameObject.WorldPosition = this.MovementHelper.DesiredLocation;
		}

		private void UpdateRotation()
		{
			this.GameObject.WorldRotation = Rotation.Slerp(this.GameObject.WorldRotation, this.MovementHelper.DesiredRotation, Time.Delta * 3f);
		}
	}
}
