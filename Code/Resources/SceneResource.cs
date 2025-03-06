// <copyright file="SceneResource.cs" company="Nathan Ford">
// Copyright (c) Nathan Ford. All rights reserved.
// </copyright>

namespace SandboxParty.Resources
{
	public enum SceneType
	{
		Board,
		Minigame
	}

	[GameResource("Scene Config", "config", "Config data relating to active scenes")]
	public partial class SceneResource : GameResource
	{
		public static IReadOnlyList<SceneResource> Boards => boards;
		internal static List<SceneResource> boards = [];

		public static IReadOnlyList<SceneResource> Minigames => minigames;
		internal static List<SceneResource> minigames = [];

		[ResourceType("scene")]
		public string Scene { get; set; }

		public SceneType SceneType { get; set; }

		[ResourceType("prefab")]
		public string PlayerPrefab { get; set; }

		public static SceneResource GetSceneResource(Scene scene, IReadOnlyList<SceneResource> dataSource)
			=> dataSource.First(source => source.Scene == scene.Source.ResourcePath);

		public SceneFile GetSceneFile()
			=> ResourceLibrary.Get<SceneFile>(this.Scene);

		public GameObject GetPlayerPrefab()
			=> GameObject.GetPrefab(this.PlayerPrefab);

		protected override void PostLoad()
		{
			base.PostLoad();

			switch (this.SceneType)
			{
				case SceneType.Board:
					if (!boards.Contains(this))
						boards.Add(this);
					break;
				case SceneType.Minigame:
					if (!minigames.Contains(this))
						minigames.Add(this);
					break;
			}
		}
	}
}
