// <copyright file="BoardPathComponent.cs" company="Nathan Ford">
// Copyright (c) Nathan Ford. All rights reserved.
// </copyright>


// <copyright file="BoardPathComponent.cs" company="Nathan Ford">
// Copyright (c) Nathan Ford. All rights reserved.
// </copyright>

namespace SandboxParty.Components.World.Board
{
	public class BoardPathComponent(BoardComponent component) : Component
	{
		public BoardComponent SelectedComponent { get; private set; }

		public bool AlreadyReachedDestination { get; private set; }

		public BoardComponent Component => component;

		public List<BoardComponent> NextComponents => [.. this.Component?.NextComponent.Select(x => x.GetComponent<BoardComponent>()) ?? []];

		public bool SelectionMade => this.SelectedComponent != null;

		public bool SelectionRequired => this.NextComponents?.Count > 1;

		public bool MoveNext(BoardComponent requestedComponent, out BoardPathComponent nextTile)
		{
			nextTile = null;

			if (this.SelectionMade)
			{
				return false;
			}

			if (this.SelectionRequired)
			{
				if (requestedComponent == null || requestedComponent.IsValid() == false)
				{
					return false;
				}

				if (this.NextComponents.Contains(requestedComponent) == false)
				{
					return false;
				}

				this.SelectedComponent = requestedComponent;
			}
			else
			{
				this.SelectedComponent = NextComponents.First();
			}

			nextTile = new BoardPathComponent(SelectedComponent);

			return true;
		}

		public bool Reached(Vector3 currentPosition, out bool justReached)
		{
			justReached = false;

			if (this.SelectionMade)
			{
				return AlreadyReachedDestination = true;
			}
			
			var reachedDestination = currentPosition.Distance(Component.WorldPosition) <= 50;
			justReached = reachedDestination && !this.AlreadyReachedDestination;

			return this.AlreadyReachedDestination = reachedDestination;
		}
	}
}
