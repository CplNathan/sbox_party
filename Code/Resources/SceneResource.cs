// <copyright file="SceneResource.cs" company="Nathan Ford">
// Copyright (c) Nathan Ford. All rights reserved.
// </copyright>

using SandboxParty.Enums;

namespace SandboxParty.Resources
{
	[GameResource("Scene Config", "config", "Config data relating to active scenes")]
	public partial class SceneResource : GameResource
	{
		private static readonly List<SceneResource> _boardList = [];

		private static readonly List<SceneResource> _minigameList = [];

		public static IReadOnlyList<SceneResource> Boards => _boardList;

		public static IReadOnlyList<SceneResource> Minigames => _minigameList;

		[ResourceType("scene")]
		public string Scene { get; set; }

		public SceneType SceneType { get; set; }

		[ResourceType("prefab")]
		public string PlayerPrefab { get; set; }

		public static SceneResource GetSceneResource(Scene scene, IReadOnlyList<SceneResource> dataSource)
			=> dataSource.First(source => source.Scene == scene.Source.ResourcePath);

		public SceneFile GetSceneFile()
			=> ResourceLibrary.Get<SceneFile>(Scene);

		public GameObject GetPlayerPrefab()
			=> GameObject.GetPrefab(PlayerPrefab);

		protected override void PostLoad()
		{
			base.PostLoad();

			switch (SceneType)
			{
				case SceneType.Board:
					if (!_boardList.Contains(this))
					{
						_boardList.Add(this);
					}

					break;
				case SceneType.Minigame:
					if (!_minigameList.Contains(this))
					{
						_minigameList.Add(this);
					}

					break;
			}
		}
	}
}
