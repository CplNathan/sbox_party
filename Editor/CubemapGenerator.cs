using System;
using System.Drawing;
using System.IO;

namespace SandboxParty
{


	[EditorTool]
	[Title( "Cubemap Generator" )]
	[Icon( "square" )]
	public class CubemapGenerator : EditorTool
	{
		private record struct CubemapData( Vector3 CameraDirection, Point CubeOffset );

		private Texture Cubemap { get; set; }

		private static readonly int CubemapFaceSize = 512;

		private static readonly List<CubemapData> Directions = new()
		{
			{ new CubemapData(Vector3.Left, new Point( 0, CubemapFaceSize) ) },
			{ new CubemapData(Vector3.Forward, new Point( CubemapFaceSize, CubemapFaceSize ) ) },
			{ new CubemapData(Vector3.Up, new Point( CubemapFaceSize, 0) ) },
			{ new CubemapData(Vector3.Down, new Point( CubemapFaceSize, CubemapFaceSize * 2) )},
			{ new CubemapData(Vector3.Right, new Point( CubemapFaceSize * 2, CubemapFaceSize) ) },
			{ new CubemapData(Vector3.Backward, new Point( CubemapFaceSize * 3, CubemapFaceSize) ) },
		};

		public override void OnEnabled()
		{
			base.OnDisabled();

			var window = new WidgetWindow( SceneOverlay );
			window.Layout = Layout.Column();
			window.Layout.Margin = 16;
			window.WindowTitle = "Cubemap Generator";

			var textureWidget = new TextureWidget();
			textureWidget.FixedWidth = 512;
			textureWidget.FixedHeight = 512;

			var buttonWidget = new Button( "Capture Cube" );
			buttonWidget.Pressed = () => RenderCubemap( textureWidget );

			var saveButton = new Button( "Save Cubemap" );
			saveButton.Pressed = () =>
			{
				if ( Cubemap == null )
					return;

				saveButton.Pressed = () =>
				{
					if ( Cubemap == null )
						return;

					var fullPath = EditorUtility.SaveFileDialog( "Save Cubemap Location", "png", Path.Combine( MainAssetBrowser.Instance.CurrentLocation.Path, "Cubemap" ) );
					if ( !string.IsNullOrEmpty( fullPath ) )
					{
						File.WriteAllBytes( fullPath, Cubemap.GetBitmap( 0 ).ToPng() );
					}
				};
			};

			window.Layout.Add( buttonWidget );
			window.Layout.Add( textureWidget );
			window.Layout.Add( saveButton );

			AddOverlay( window, TextFlag.RightTop, 10 );
		}

		public override void OnUpdate()
		{
			base.OnUpdate();

		}

		private void RenderCubemap( TextureWidget output )
		{
			var cubeTexture = Texture.CreateRenderTarget().WithSize( CubemapFaceSize * 4, CubemapFaceSize * 3 ).Create();
			var activeObject = GetSelectedComponent<Component>();

			var cameraObject = Scene.CreateObject();
			try
			{
				var camera = cameraObject.AddComponent<CameraComponent>();
				camera.Orthographic = true;
				camera.OrthographicHeight = CubemapFaceSize / 2;
				camera.CustomSize = new Vector2( 1, 1 );

				var ambientOcclusion = cameraObject.AddComponent<AmbientOcclusion>();

				foreach ( var data in Directions )
				{
					cameraObject.WorldPosition = activeObject.WorldPosition;
					cameraObject.WorldRotation = Rotation.LookAt( data.CameraDirection, Vector3.Up );

					var texture = Texture.CreateRenderTarget().WithSize( CubemapFaceSize, CubemapFaceSize ).Create();
					camera.RenderToTexture( texture );

					cubeTexture.Update( texture.GetPixels(), data.CubeOffset.X, data.CubeOffset.Y, CubemapFaceSize, CubemapFaceSize );
				}

				output.Texture = cubeTexture;
				output.Update();

				Cubemap = cubeTexture;
			}
			catch ( Exception )
			{
			}
			finally
			{
				if ( cameraObject?.IsValid() == true )
					cameraObject.Destroy();
			}
		}
	}
}
