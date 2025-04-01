// <copyright file="BoardCharacter.cs" company="Nathan Ford">
// Copyright (c) Nathan Ford. All rights reserved.
// </copyright>

using Sandbox.Citizen;
using SandboxParty.Components.World.Board;
using SandboxParty.Events.Board;
using GameManager = SandboxParty.Managers.GameManager;

namespace SandboxParty.Components.Character.Board
{
	public class BoardCharacter : BaseCharacter, IBoardCharacterEvent, IBoardCharacterInputEvent
	{
		public bool IsOurTurn { get => GameManager.Current.BoardState?.CurrentTurn == this; }

		[RequireComponent]
		public BoardCharacterMovementAgent MovementHelper { get; set; }

		[RequireComponent]
		public CitizenAnimationHelper AnimationHelper { get; set; }

		[RequireComponent]
		public BoardCharacterPathRendering PathRendering { get; set; }

		[RequireComponent]
		public BoardCharacterDice Dice { get; set; }

		protected override void OnUpdate()
		{
			base.OnUpdate();

			if (!this.GameObject.IsValid())
			{
				return;
			}

			UpdateAnimations();
			UpdateLocation();
			UpdateRotation();
			UpdateRendering();

			if (IsProxy)
			{
				return;
			}

			Gizmo.Draw.ScreenText($"You rolled {Dice.LastRoll}", new Vector2(50, 50));

			if (Input.Pressed("jump"))
			{
				IBoardCharacterInputEvent.PostToGameObject(this.GameObject, x => x.OnDiceRoll());
			}

			if (Input.Pressed("attack1"))
			{
				var closestComponent = MovementHelper.CurrentTile.NextComponents.OrderBy(component => component.WorldPosition.Distance(Scene.Camera.ScreenToWorld(Gizmo.CursorPosition))).First();
				IBoardCharacterInputEvent.PostToGameObject(this.GameObject, x => x.OnDirectionDecided(closestComponent));
			}
		}

		private void UpdateAnimations()
		{
			AnimationHelper.WithVelocity(MovementHelper.Velocity);
			AnimationHelper.WithWishVelocity(MovementHelper.WishVelocity);
			AnimationHelper.Sitting = IsOurTurn ? CitizenAnimationHelper.SittingStyle.None : CitizenAnimationHelper.SittingStyle.Floor;
		}

		private void UpdateLocation()
		{
			GameObject.WorldPosition = MovementHelper.DesiredLocation;
		}

		private void UpdateRotation()
		{
			GameObject.WorldRotation = Rotation.Slerp(GameObject.WorldRotation, MovementHelper.DesiredRotation, Time.Delta * 3f);
		}

		private void UpdateRendering()
		{
			PathRendering.CurrentTile = MovementHelper.CurrentTile;
			PathRendering.Steps = MovementHelper.Steps;
		}

		[Rpc.Host(Flags = NetFlags.HostOnly)]
		void IBoardCharacterEvent.OnDestinationReached()
		{
			if (!OnDestinationReached_Validate())
			{
				return;
			}

			IBoardTurnEvent.Post(x => x.OnDestinationReached(this));
		}

		bool OnDestinationReached_Validate()
		{
			if (!Networking.IsHost)
			{
				return false;
			}

			return true;
		}

		[Rpc.Host(Flags = NetFlags.OwnerOnly)]
		void IBoardCharacterInputEvent.OnDiceRoll()
		{
			if (!OnDiceRoll_Validate())
			{
				Log.Warning($"Failed to validate OnDiceRoll RPC");
				return;
			}

			Vector3 forwardVector = AnimationHelper.Target.WorldRotation.Forward;
			Vector3 upVector = Vector3.Up;
			Vector3 vector = GameObject.WorldPosition + (forwardVector * 50) + (upVector * 25);

			Dice.RollDice(vector).ContinueWith(newRoll =>
			{
				MovementHelper.Steps += newRoll.Result;
			});
		}

		bool OnDiceRoll_Validate()
		{
			if (!Networking.IsHost)
			{
				return false;
			}

			return IsOurTurn;
		}

		[Rpc.Host(Flags = NetFlags.OwnerOnly)]
		void IBoardCharacterInputEvent.OnDirectionDecided(BoardComponent boardComponent)
		{
			if (!OnDirectionDecided_Validate(boardComponent))
			{
				Log.Warning($"Failed to validate OnDirectionDecided RPC");
				return;
			}

			MovementHelper.RequestedComponent = boardComponent;
		}

		bool OnDirectionDecided_Validate(BoardComponent boardComponent)
		{
			if (!Networking.IsHost)
			{
				return false;
			}

			return MovementHelper.CurrentTile.NextComponents.Contains(boardComponent);
		}

		[Rpc.Host(Flags = NetFlags.OwnerOnly)]
		void IBoardCharacterInputEvent.OnEndTurn()
		{
			if (!EndTurn_Validate())
			{
				Log.Warning($"Failed to validate OnEndTurn RPC");
				return;
			}

			IBoardTurnEvent.Post(x => x.OnTurnEnded(this));
		}

		bool EndTurn_Validate()
		{
			if (!Networking.IsHost)
			{
				return false;
			}

			return IsOurTurn;
		}
	}
}
