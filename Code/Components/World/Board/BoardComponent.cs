﻿// <copyright file="BoardComponent.cs" company="Nathan Ford">
// Copyright (c) Nathan Ford. All rights reserved.
// </copyright>

namespace SandboxParty.Components.World.Board
{
	[Title("Board Component")]
	public class BoardComponent : Component, Component.ExecuteInEditor
	{
		[Property]
		public GameObject[] NextComponent { get; set; }

		[Property]
		public bool IsSpawn { get; set; }

		protected override void OnUpdate()
		{
			base.OnUpdate();

			Gizmo.Draw.Lines(NextComponent.Select(x => new Line(WorldPosition, x.WorldPosition)));
		}
	}
}
