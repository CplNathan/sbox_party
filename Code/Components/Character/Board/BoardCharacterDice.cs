﻿// <copyright file="BoardCharacterDice.cs" company="Nathan Ford">
// Copyright (c) Nathan Ford. All rights reserved.
// </copyright>

using System;
using System.Threading;
using System.Threading.Tasks;
using Sandbox.Diagnostics;
using SandboxParty.Components.World.Board.Dice;

namespace SandboxParty.Components.Character.Board
{
	[Title("Board Dice")]
	public class BoardCharacterDice : Component
	{
		public List<GameObject> ValidDice { get => [.. _spawnedDice?.Where(x => x.IsValid) ?? []]; }

		[Property]
		public GameObject DicePrefab { get; init; }

		[Property]
		public int DicePerRoll { get; init; } = 2;

		[Property]
		public int DiceLifetimeSeconds { get; init; } = 5;

		[Property]
		public int Multiplier { get; init; } = 1;

		[Sync(Flags = SyncFlags.FromHost)]
		public int LastRoll { get; private set; } = 0;

		private List<GameObject> _spawnedDice = [];

		private CancellationTokenSource _cancellationTokenSource = new();

		public async Task<int> RollDice(Vector3 effectLocation)
		{
			var newRoll = await PerformEffects(DicePerRoll, effectLocation);
			LastRoll = newRoll >= 2 ? newRoll : Random.Shared.Next(DicePerRoll, DicePerRoll * 6);
			return LastRoll;
		}

		public async Task<int> PerformEffects(int count, Vector3 vector)
		{
			GameObject[] dice = CloneDice(count, vector);

			try
			{
				await Task.DelaySeconds(DiceLifetimeSeconds, _cancellationTokenSource.Token);

				var diceReaders = dice.Select(x => x.GetComponent<BoardDiceReader>()).ToList();
				return diceReaders.Sum(x => x.ReadNumber());
			}
			catch (OperationCanceledException ex)
			{
				Log.Warning(ex, "Cancelled dice roll before expiration");
			}
			finally
			{
				DestroyDice(dice);
			}

			return -1;
		}

		public GameObject[] CloneDice(int count, Vector3 vector)
		{
			Assert.NotNull(DicePrefab);

			GameObject[] newDice = new GameObject[count];
			_spawnedDice?.EnsureCapacity(_spawnedDice.Count + count);

			for (int i = 0; i < count; i++)
			{
				var diceObject = DicePrefab.Clone(vector, Rotation.Random);
				diceObject.NetworkSpawn();
				diceObject.GetComponent<Rigidbody>().ApplyForceAt(Vector3.Random, -(WorldPosition - vector) * 20000);

				newDice[i] = diceObject;
				_spawnedDice?.Add(diceObject);
			}

			return newDice;
		}

		protected void Cleanup()
		{
			_cancellationTokenSource?.Cancel();
			_cancellationTokenSource = null;

			ValidDice?.ForEach(x => x.Destroy());
			_spawnedDice = null;
		}

		protected override void OnStart()
		{
			base.OnStart();

			_cancellationTokenSource ??= new();
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();

			Cleanup();
		}

		protected override void OnUpdate()
		{
			base.OnUpdate();
		}

		private void DestroyDice(GameObject[] destroyDice)
		{
			foreach (var dice in destroyDice)
			{
				if (dice.IsValid)
				{
					dice.Destroy();
				}

				_spawnedDice?.Remove(dice);
			}
		}
	}
}
