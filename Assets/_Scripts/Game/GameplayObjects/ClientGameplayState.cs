using CosmicShore.Core;
using CosmicShore.Game;
using CosmicShore.Game.Arcade;
using Unity.Netcode;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.Assertions;


namespace CosmicShore.Game.GameplayObjects
{
    public class ClientGameplayState : NetworkBehaviour
    {
        [SerializeField]
        MiniGame _miniGame;

        [ClientRpc]
        public void InitializeAndSetupPlayer_ClientRpc(ClientRpcParams clientRpcParams = default)
        {
            if (NetworkManager.Singleton.ConnectedClients.Count == 0)
            {
                Debug.LogError($"No clients found in network manager!");
                return;
            }

            foreach (var kvp in NetworkManager.Singleton.ConnectedClients)
            {
                ulong clientId = kvp.Key;
                NetworkObject playerNetworkObject = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(clientId);
                Assert.IsTrue(playerNetworkObject, $"Matching player network object for client {clientId} not found!");

                playerNetworkObject.TryGetComponent(out NetworkPlayer networkPlayer);
                Assert.IsTrue(networkPlayer, $"Network Player not found for client {clientId}!");
                if (clientId == OwnerClientId)
                {
                    _miniGame.InitializePlayer(networkPlayer);
                }

                NetworkShip networkShip = NetworkShipClientCache.GetShip(clientId);
                Assert.IsTrue(networkShip, $"Network ship not found for client {clientId}!");

                // NetworkShip networkShip = NetworkShipClientCache.OwnShip;
                // _player.Setup(networkShip.Ship);
                networkPlayer.Setup(networkShip.Ship, clientId == OwnerClientId);
                // networkShip.SetPlayerUUIDToTrailSpawner_ClientRpc(_player.PlayerUUID);
            }

            GameManager.Instance.WaitOnPlayerLoading();
        }
    }
}
