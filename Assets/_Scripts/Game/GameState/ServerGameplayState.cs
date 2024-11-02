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

        private NetcodeHooks m_NetcodeHooks;
        private List<Transform> m_PlayerSpawnPointsList = null;

        [Inject]
        private SceneNameListSO m_SceneNameList;

        protected override void Awake()
        {
            base.Awake();
            m_NetcodeHooks = GetComponent<NetcodeHooks>();
            m_NetcodeHooks.OnNetworkSpawnHook += OnNetworkSpawn;
            m_NetcodeHooks.OnNetworkDespawnHook += OnNetworkDespawn;
        }

        private void OnNetworkSpawn()
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                enabled = false;
                return;
            }

            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;
            SessionManager<SessionPlayerData>.Instance.OnSessionStarted();
        }

        private void OnNetworkDespawn()
        {
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnect;
        }

        protected override void OnDestroy()
        {
            if (m_NetcodeHooks)
            {
                m_NetcodeHooks.OnNetworkSpawnHook -= OnNetworkSpawn;
                m_NetcodeHooks.OnNetworkDespawnHook -= OnNetworkDespawn;
            }

            base.OnDestroy();
        }

        void OnClientDisconnect(ulong clientId)
        {
            if (clientId == NetworkManager.Singleton.LocalClientId)
            {
                // This client (which is a server) disconnects. We should go back to the character select screen.
                SceneLoaderWrapper.Instance.LoadScene(m_SceneNameList.MainMenuScene, useNetworkSceneManager: true);
            }
        }

    }
}
