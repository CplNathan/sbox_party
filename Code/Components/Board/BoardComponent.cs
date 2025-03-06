// <copyright file="BoardComponent.cs" company="Nathan Ford">
// Copyright (c) Nathan Ford. All rights reserved.
// </copyright>

namespace SandboxParty.Components.Board
{
	internal struct BoardPathComponent(BoardComponent component)
	{
		public BoardComponent SelectedComponent { get; private set; }

		public readonly BoardComponent Component => component;

		public readonly BoardComponent[] NextComponents => [.. component.NextComponent.Select(x => x.GetComponent<BoardComponent>())];

		public readonly bool SelectionMade => this.SelectedComponent != null;

		public readonly bool SelectionRequired => this.NextComponents.Length > 1;

		private bool DestinationNotificationSent { get; set; } = false;

		public bool MoveNext(BoardComponent requestedComponent, out BoardPathComponent? nextTile)
		{
			nextTile = null;

			if (this.SelectionMade)
				return false;

			if (this.SelectionRequired)
			{
				if (!requestedComponent.IsValid())
					return false;

				if (this.NextComponents.Contains(requestedComponent))
				{
					this.SelectedComponent = requestedComponent;
				}
			}
			else
			{
				this.SelectedComponent = this.NextComponents.First();
			}

			nextTile = new BoardPathComponent(this.SelectedComponent);

			return true;
		}

		public readonly bool Reached(Vector3 currentPosition, int maxDistance = 25)
		{
			if (this.SelectionMade)
			{
				return true;
			}

			return currentPosition.Distance(component.WorldPosition) <= maxDistance;
		}

		public bool CanSendDestinationNotification(GameObject target)
		{
			if (this.SelectionMade || this.DestinationNotificationSent)
			{
				return false;
			}

			this.DestinationNotificationSent = true;
			return true;
		}
	}

	[Title("Board Component")]
	public class BoardComponent : Component, Component.ExecuteInEditor
	{
		[Property]
		public GameObject[] NextComponent { get; set; }

		protected override void OnUpdate()
		{
			base.OnUpdate();

			Gizmo.Draw.Lines(this.NextComponent.Select(x => new Line(this.WorldPosition, x.WorldPosition)));
		}
	}
}
