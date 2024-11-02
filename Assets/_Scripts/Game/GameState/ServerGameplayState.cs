﻿using CosmicShore.Game.Arcade;
using CosmicShore.Game.GameplayObjects;
using CosmicShore.NetworkManagement;
using CosmicShore.Utilities;
using CosmicShore.Utilities.Network;
using System.Collections.Generic;
using Unity.Multiplayer.Samples.Utilities;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;
using VContainer;



namespace CosmicShore.Gameplay.GameState
{
    [RequireComponent(typeof(NetcodeHooks))]
    public class ServerGameplayState : GameStateBehaviour
    {
        public override GameState ActiveState => GameState.Gameplay;

        [SerializeField]
        ClientGameplayState _clientGameplayState;

        [SerializeField]
        MiniGame _miniGame;

        [SerializeField]
        NetworkObject _shipPrefab;

        [SerializeField]
        [Tooltip("A collection of locations for spawning players")]
        private Transform[] _playerSpawnPoints;

        /*[SerializeField]
        private NetworkPlayerService m_NetworkPlayerService;*/

        /// <summary>
        /// Has the ServerGameplayState already hit its initial spawn? (i.e. spawned players following load from character select).
        /// </summary>
        public bool InitialSpawnDone { get; private set; }

        private NetcodeHooks _netcodeHooks;
        private List<Transform> _playerSpawnPointsList = null;

        [Inject]
        private SceneNameListSO _sceneNameList;

        protected override void Awake()
        {
            base.Awake();
            _netcodeHooks = GetComponent<NetcodeHooks>();
            _netcodeHooks.OnNetworkSpawnHook += OnNetworkSpawn;
            _netcodeHooks.OnNetworkDespawnHook += OnNetworkDespawn;
        }

        private void OnNetworkSpawn()
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                enabled = false;
                return;
            }

            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += OnLoadEventCompleted;
            NetworkManager.Singleton.SceneManager.OnSynchronizeComplete += OnSynchronizeComplete;

            SessionManager<SessionPlayerData>.Instance.OnSessionStarted();
        }

        private void OnNetworkDespawn()
        {
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnect;
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= OnLoadEventCompleted;
            NetworkManager.Singleton.SceneManager.OnSynchronizeComplete -= OnSynchronizeComplete;
        }

        protected override void OnDestroy()
        {
            if (_netcodeHooks)
            {
                _netcodeHooks.OnNetworkSpawnHook -= OnNetworkSpawn;
                _netcodeHooks.OnNetworkDespawnHook -= OnNetworkDespawn;
            }

            base.OnDestroy();
        }

        void OnSynchronizeComplete(ulong clientId)
        {
            if (InitialSpawnDone && !NetworkShipClientCache.GetShip(clientId))
            {
                //somebody joined after the initial spawn. This is a Late Join scenario. This player may have issues
                //(either because multiple people are late-joining at once, or because some dynamic entities are
                //getting spawned while joining. But that's not something we can fully address by changes in
                //ServerMultiplayGameState.
                
                SpawnNetworkPlayerForEachClients(clientId, true);
            }
        }

        void OnLoadEventCompleted(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
        {
            if (!InitialSpawnDone && loadSceneMode == LoadSceneMode.Single)
            {
                InitialSpawnDone = true;
                foreach (var kvp in NetworkManager.Singleton.ConnectedClients)
                {
                    SpawnNetworkPlayerForEachClients(kvp.Key, false);
                }
            }
        }

        void OnClientDisconnect(ulong clientId)
        {
            if (clientId == NetworkManager.Singleton.LocalClientId)
            {
                // This client (which is a server) disconnects. We should go back to the character select screen.
                SceneLoaderWrapper.Instance.LoadScene(_sceneNameList.CharSelectScene, useNetworkSceneManager: true);
            }
        }

        void SpawnNetworkPlayerForEachClients(ulong clientId, bool lateJoin)
        {
            NetworkObject playerNetworkObject = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(clientId);
            /*_player = m_MiniGame.InitializePlayer() as NetworkPlayer;
            _player.Setup();
            GameManager.Instance.WaitOnPlayerLoading();*/

            NetworkObject networkShip = Instantiate(_shipPrefab);
            Assert.IsTrue(networkShip, $"Matching ship network object for client {clientId} not found!");

            var persistentPlayerExists = playerNetworkObject.TryGetComponent(out PersistentPlayer persistentPlayer);
            Assert.IsTrue(persistentPlayerExists,
                $"Matching persistent PersistentPlayer for client {clientId} not found!");

            // if reconnecting, set the player's position and rotation to its previous state
            if (lateJoin)
            {
                SessionPlayerData? sessionPlayerData = SessionManager<SessionPlayerData>.Instance.GetPlayerData(clientId);
                if (sessionPlayerData is { HasCharacterSpawned: true })
                {
                    networkShip.transform.SetPositionAndRotation(sessionPlayerData.Value.PlayerPosition, sessionPlayerData.Value.PlayerRotation);
                }
            }
            else // else spawn the player at a random spawn point
            {
                Transform spawnPoint = GetRandomSpawnPoint();
                networkShip.transform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);
            }

            networkShip.SpawnWithOwnership(clientId, true);

            ClientRpcParams clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new ulong[] { clientId }
                }
            };
            _clientGameplayState.InitializeAndSetupPlayer_ClientRpc(clientRpcParams);

            // pass name from persistent player to avatar
            /*if (newPlayer.TryGetComponent(out NetworkNameState networkNameState))                 // NetworkNameState
            {
                networkNameState.Name.Value = persistentPlayer.NetworkNameState.Name.Value;
            }*/

            // m_NetworkPlayerService.ConfigureNetworkPlayer_ClientRpc(clientId);
        }

        /// <summary>
        /// This need to be called on the server to set separate position for each client.
        /// </summary>
        private Transform GetRandomSpawnPoint()
        {
            Transform spawnPoint;

            if (_playerSpawnPointsList == null || _playerSpawnPointsList.Count == 0)
            {
                _playerSpawnPointsList = new List<Transform>(_playerSpawnPoints);
            }

            Debug.Assert(_playerSpawnPointsList.Count > 0,
                $"PlayerSpawnPoints array should have at least 1 spawn points.");

            int index = Random.Range(0, _playerSpawnPointsList.Count);
            spawnPoint = _playerSpawnPointsList[index];
            _playerSpawnPointsList.RemoveAt(index);

            return spawnPoint;
        }
    }
}
