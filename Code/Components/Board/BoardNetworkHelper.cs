﻿using SandboxParty.Components.Board.Character;
using SandboxParty.GameManager;
using System.Threading.Tasks;

namespace SandboxParty.Components.Board
{
	[Title( "Board Network Helper" )]
	public class BoardNetworkHelper : Component, Component.INetworkSpawn, Component.INetworkListener, Component.ExecuteInEditor
	{
		[Property] public GameObject PlayerPrefab { get; set; }

		[Property] public GameObject SpawnPoint { get; set; }

		public BoardComponent SpawnComponent { get => SpawnPoint.IsValid() ? SpawnPoint.Components.Get<BoardComponent>() : null; }

		[Sync] public BoardGameState GameState { get; set; }

		protected override void OnStart()
		{
			base.OnStart();

			GameObject.NetworkSpawn();
		}

		void INetworkSpawn.OnNetworkSpawn( Connection owner )
		{
			GameState = GameObject.AddComponent<BoardGameState>();
			GameObject.Network.Refresh();
		}

		bool INetworkListener.AcceptConnection( Connection channel, ref string reason )
		{
			//if ( _gameManager.State.CurrentState != GameState.Waiting )
			//reason += "Game has already started";

			return string.IsNullOrWhiteSpace( reason );
		}

		void INetworkListener.OnActive( Connection channel )
		{
			Log.Info( $"Player '{channel.DisplayName}' has joined the game" );

			if ( !PlayerPrefab.IsValid() )
				return;



			//
			// Find a spawn location for this player
			//
			var startLocation = SpawnComponent.GameObject.WorldTransform.WithScale( 1 ).WithRotation( new Rotation() );

			// Spawn this object and make the client the owner
			var player = PlayerPrefab.Clone( startLocation, name: $"Player - {channel.DisplayName}" );
			player.NetworkSpawn( channel );

			GameState.Players[channel.Id] = player.GetComponentInChildren<BoardCharacter>();
		}

		void INetworkListener.OnDisconnected( Connection channel )
		{
			GameState.Players.Remove( channel.Id );
		}
	}
}
