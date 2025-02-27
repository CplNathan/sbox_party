namespace SandboxParty.Components.Board
{
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
