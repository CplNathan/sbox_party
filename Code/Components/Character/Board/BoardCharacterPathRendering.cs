using SandboxParty.Structs;

namespace SandboxParty.Components.Character.Board
{
    public class BoardCharacterPathRendering : Component
    {
		public BoardTile CurrentTile { set => _currentTile = value; }

		public int Steps { get; set; }

		private BoardTile _currentTile;

		protected override void OnUpdate()
		{
			base.OnUpdate();

			RenderPath(_currentTile, Steps);
			RenderBranch();
		}

		private void RenderPath(BoardTile tile, int steps)
		{
			var previousTile = tile;

			for (int step = 1; step < Steps; step++)
			{
				var requiresDecision = tile.SelectionRequired && !tile.SelectionMade;
				if (requiresDecision)
				{
					for (int branch = 0; branch < previousTile.NextComponents.Count; branch++)
					{
						Gizmo.Draw.Color = (branch % 2) == 0 ? Color.Red : Color.Blue;
						RenderPath(new BoardTile(previousTile.NextComponents[branch]), steps - step - 1);
					}

					return;
				}

				Gizmo.Draw.Text($"{step}", new Transform(previousTile.Component.WorldPosition, Rotation.LookAt(Scene.Camera.WorldRotation.Forward, Vector3.Up)), size: 24);

				previousTile = new BoardTile(previousTile.NextComponents[0]);
			}
		}

		private void RenderBranch()
		{
			var currentComponent = _currentTile.Component;

			if (_currentTile.SelectionRequired && !_currentTile.SelectionMade)
			{
				foreach (var nextComponent in _currentTile.NextComponents)
				{
					var forwardDirection = Rotation.LookAt(nextComponent.WorldPosition - currentComponent.WorldPosition).Forward;

					Gizmo.Draw.Color = Color.White;
					Gizmo.Draw.Arrow(currentComponent.WorldPosition + (forwardDirection * 10), currentComponent.WorldPosition + (forwardDirection * 25));
				}
			}
		}
	}
}
