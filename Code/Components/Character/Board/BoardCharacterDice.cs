// <copyright file="BoardCharacterDice.cs" company="Nathan Ford">
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
		private List<GameObject> spawnedDice = [];

		private CancellationTokenSource cancellationTokenSource = new();

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

		private List<GameObject> ValidDice { get => [.. this.spawnedDice?.Where(x => x.IsValid) ?? []]; }

		public async Task<int> RollDice(Vector3 effectLocation)
		{
			var newRoll = await this.PerformEffects(this.DicePerRoll, effectLocation);
			this.LastRoll = newRoll >= 2 ? newRoll : Random.Shared.Next(this.DicePerRoll, this.DicePerRoll * 6);
			return this.LastRoll;
		}

		public async Task<int> PerformEffects(int count, Vector3 vector)
		{
			GameObject[] dice = this.CloneDice(count, vector);

			try
			{
				await this.Task.DelaySeconds(this.DiceLifetimeSeconds, this.cancellationTokenSource.Token);

				var diceReaders = dice.Select(x => x.GetComponent<BoardDiceReader>()).ToList();
				return diceReaders.Sum(x => x.ReadNumber());
			}
			catch (OperationCanceledException ex)
			{
				Log.Warning(ex, "Cancelled dice roll before expiration");
			}
			finally
			{
				this.DestroyDice(dice);
			}

			return -1;
		}

		public GameObject[] CloneDice(int count, Vector3 vector)
		{
			Assert.NotNull(this.DicePrefab);

			GameObject[] newDice = new GameObject[count];
			this.spawnedDice?.EnsureCapacity(this.spawnedDice.Count + count);

			for (int i = 0; i < count; i++)
			{
				var diceObject = this.DicePrefab.Clone(vector, Rotation.Random);
				diceObject.NetworkSpawn();
				diceObject.GetComponent<Rigidbody>().ApplyForceAt(Vector3.Random, -(this.WorldPosition - vector) * 20000);

				newDice[i] = diceObject;
				this.spawnedDice?.Add(diceObject);
			}

			return newDice;
		}

		protected void Cleanup()
		{
			this.cancellationTokenSource?.Cancel();
			this.cancellationTokenSource = null;

			this.ValidDice?.ForEach(x => x.Destroy());
			this.spawnedDice = null;
		}

		protected override void OnStart()
		{
			base.OnStart();

			this.cancellationTokenSource ??= new();
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();

			this.Cleanup();
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

				this.spawnedDice?.Remove(dice);
			}
		}
	}
}
