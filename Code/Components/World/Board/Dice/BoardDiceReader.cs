// <copyright file="BoardDiceReader.cs" company="Nathan Ford">
// Copyright (c) Nathan Ford. All rights reserved.
// </copyright>

namespace SandboxParty.Components.World.Board.Dice
{
	[Title("Dice Reader")]
	public class BoardDiceReader : Component
	{
		public int ReadNumber()
		{
			var diceNumbers = GetComponentsInChildren<BoardDiceNumber>();

			var normalizedVectors = diceNumbers.ToDictionary(x => x.DiceNumber, x => GameObject.WorldPosition - x.WorldPosition);
			var sortedVectors = normalizedVectors.OrderByDescending(x => Vector3.DistanceBetween(x.Value, Vector3.Up));

			return sortedVectors.First().Key;
		}
	}
}
