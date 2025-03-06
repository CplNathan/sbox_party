using SandboxParty.Events;

namespace SandboxParty.Components.Board
{
	internal struct BoardPathComponent( BoardComponent component )
	{
		public BoardComponent SelectedComponent { get; private set; }

		public readonly BoardComponent Component => component;

		public readonly BoardComponent[] NextComponents => [.. component.NextComponent.Select( x => x.GetComponent<BoardComponent>() )];

		public readonly bool SelectionMade => SelectedComponent != null;

		public readonly bool SelectionRequired => NextComponents.Length > 1;

		private bool DestinationNotificationSent { get; set; } = false;

		public bool MoveNext( BoardComponent requestedComponent, out BoardPathComponent? nextTile )
		{
			nextTile = null;

			if ( SelectionMade )
				return false;

			if ( SelectionRequired )
			{
				if ( !requestedComponent.IsValid() )
					return false;

				if ( NextComponents.Contains( requestedComponent ) )
				{
					SelectedComponent = requestedComponent;
				}
			}
			else
			{
				SelectedComponent = NextComponents.First();
			}

			nextTile = new BoardPathComponent( SelectedComponent );

			return true;
		}

		public readonly bool Reached( Vector3 currentPosition, int maxDistance = 25 )
		{
			if ( SelectionMade )
				return true;

			return currentPosition.Distance( component.WorldPosition ) <= maxDistance;
		}

		public bool CanSendDestinationNotification( GameObject target )
		{
			if ( SelectionMade || DestinationNotificationSent )
				return false;

			DestinationNotificationSent = true;
			return true;
		}
	}

	[Title( "Board Component" )]
	public class BoardComponent : Component, Component.ExecuteInEditor
	{
		[Property]
		public GameObject[] NextComponent { get; set; }

		protected override void OnUpdate()
		{
			base.OnUpdate();

			Gizmo.Draw.Lines( NextComponent.Select( x => new Line( WorldPosition, x.WorldPosition ) ) );
		}
	}
}
