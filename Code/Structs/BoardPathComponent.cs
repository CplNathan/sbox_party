// <copyright file="BoardPathComponent.cs" company="Nathan Ford">
// Copyright (c) Nathan Ford. All rights reserved.
// </copyright>

using System;
using SandboxParty.Components.World.Board;

namespace SandboxParty.Structs
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
			{
				return false;
			}

			if (this.SelectionRequired)
			{
				if (!requestedComponent.IsValid())
				{
					return false;
				}

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

		public bool CanSendDestinationNotification()
		{
			if (this.SelectionMade || this.DestinationNotificationSent)
			{
				return false;
			}

			this.DestinationNotificationSent = true;
			return true;
		}
	}
}
