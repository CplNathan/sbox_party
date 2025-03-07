// <copyright file="BoardPathComponent.cs" company="Nathan Ford">
// Copyright (c) Nathan Ford. All rights reserved.
// </copyright>


// <copyright file="BoardPathComponent.cs" company="Nathan Ford">
// Copyright (c) Nathan Ford. All rights reserved.
// </copyright>

using SandboxParty.Components.World.Board;

namespace SandboxParty.Structs
{
	public struct BoardTile
	{
		public BoardTile(BoardComponent component)
		{
			Component = component;
		}

		public BoardComponent Component { get; set; }

		public BoardComponent SelectedComponent { get; private set; }

		public bool ReachedDestination { get; private set; }

		public List<BoardComponent> NextComponents => [.. Component?.NextComponent.Select(x => x.GetComponent<BoardComponent>()) ?? []];

		public bool SelectionMade => SelectedComponent != null;

		public bool SelectionRequired => NextComponents?.Count > 1;

		public bool MoveNext(BoardComponent requestedComponent, out BoardTile? nextTile)
		{
			nextTile = null;

			if (SelectionMade)
			{
				return false;
			}

			if (SelectionRequired)
			{
				if (requestedComponent == null || requestedComponent.IsValid() == false)
				{
					return false;
				}

				if (NextComponents.Contains(requestedComponent) == false)
				{
					return false;
				}

				SelectedComponent = requestedComponent;
			}
			else
			{
				SelectedComponent = NextComponents.First();
			}

			nextTile = new BoardTile(SelectedComponent);

			return true;
		}

		public bool Reached(Vector3 currentPosition, out bool justReached)
		{
			justReached = false;

			if (SelectionMade)
			{
				return ReachedDestination = true;
			}
			
			var nearDestination = currentPosition.Distance(Component.WorldPosition) <= 50;
			justReached = nearDestination && !ReachedDestination;

			return ReachedDestination = nearDestination;
		}
	}
}
