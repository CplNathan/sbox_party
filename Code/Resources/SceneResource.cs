// <copyright file="SceneResource.cs" company="Nathan Ford">
// Copyright (c) Nathan Ford. All rights reserved.
// </copyright>

using SandboxParty.Enums;

namespace SandboxParty.Resources
{
	[GameResource("Scene Config", "config", "Config data relating to active scenes")]
	public partial class SceneResource : GameResource
	{
		private static readonly List<SceneResource> BoardList = [];

		private static readonly List<SceneResource> MinigameList = [];

		public static IReadOnlyList<SceneResource> Boards => BoardList;

		public static IReadOnlyList<SceneResource> Minigames => MinigameList;

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
					if (!BoardList.Contains(this))
					{
						BoardList.Add(this);
					}

					break;
				case SceneType.Minigame:
					if (!MinigameList.Contains(this))
					{
						MinigameList.Add(this);
					}

					break;
			}
		}
	}
}
