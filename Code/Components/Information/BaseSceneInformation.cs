﻿// <copyright file="BaseSceneInformation.cs" company="Nathan Ford">
// Copyright (c) Nathan Ford. All rights reserved.
// </copyright>

namespace SandboxParty.Components.Information
{
	public class BaseSceneInformation : SceneInformation
	{
		public static List<GameObject> GetSpawnPoints(Scene scene)
			=> scene.GetComponentInChildren<BaseSceneInformation>().SpawnPoints;

		[Property]
		public List<GameObject> SpawnPoints { get; set; }

		[Property]
		public List<GameObject> RespawnPoints { get; set; }
	}
}
