// <copyright file="BoardCharacterDice.cs" company="Nathan Ford">
// Copyright (c) Nathan Ford. All rights reserved.
// </copyright>

using Sandbox.Diagnostics;
using SandboxParty.Components.Board.Dice;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SandboxParty.Components.Character.Board
{
	[Title("Board Dice")]
	public class BoardCharacterDice : Component
	{
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

		private List<GameObject> ValidDice { get => [.. this._spawnedDice?.Where(x => x.IsValid) ?? []]; }

		private List<GameObject> _spawnedDice = [];

		private CancellationTokenSource _cancellationTokenSource = new();

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
				await this.Task.DelaySeconds(this.DiceLifetimeSeconds, this._cancellationTokenSource.Token);

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
			this._spawnedDice?.EnsureCapacity(this._spawnedDice.Count + count);

			for (int i = 0; i < count; i++)
			{
				var diceObject = this.DicePrefab.Clone(vector, Rotation.Random);
				diceObject.NetworkSpawn();
				diceObject.GetComponent<Rigidbody>().ApplyForceAt(Vector3.Random, -(this.WorldPosition - vector) * 20000);

				newDice[i] = diceObject;
				this._spawnedDice?.Add(diceObject);
			}

			return newDice;
		}

		private void DestroyDice(GameObject[] destroyDice)
		{
			foreach (var dice in destroyDice)
			{
				if (dice.IsValid)
					dice.Destroy();

				this._spawnedDice?.Remove(dice);
			}
		}

		private void Cleanup()
		{
			this._cancellationTokenSource?.Cancel();
			this._cancellationTokenSource = null;

			this.ValidDice?.ForEach(x => x.Destroy());
			this._spawnedDice = null;
		}

		protected override void OnStart()
		{
			base.OnStart();

			this._cancellationTokenSource ??= new();
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
	}
}
