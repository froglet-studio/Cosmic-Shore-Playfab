﻿using CosmicShore.Core;
using Unity.Netcode;
using UnityEngine.Assertions;


namespace CosmicShore.Game.GameplayObjects
{
    public class ClientGameplayState : NetworkBehaviour
    {
        [ClientRpc]
        public void InitializeAndSetupPlayer_ClientRpc(ClientRpcParams clientRpcParams = default)
        {
            foreach (NetworkPlayer networkPlayer in NetworkPlayer.NppList)
            {
                NetworkShip networkShip = NetworkShipClientCache.GetShip(networkPlayer.OwnerClientId);
                Assert.IsTrue(networkShip, $"Network ship not found for client {networkPlayer.OwnerClientId}!");

                networkPlayer.Setup(networkShip);
            }

            GameManager.UnPauseGame();
            GameManager.Instance.WaitOnPlayerLoading();
        }
    }
}
