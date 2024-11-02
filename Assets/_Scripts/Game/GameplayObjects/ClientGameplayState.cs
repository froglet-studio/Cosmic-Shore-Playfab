using CosmicShore.Core;
using CosmicShore.Game;
using CosmicShore.Game.Arcade;
using Unity.Netcode;
using UnityEngine;


namespace CosmicShore.Game.GameplayObjects
{
    public class ClientGameplayState : NetworkBehaviour
    {
        [SerializeField]
        MiniGame _miniGame;

        private NetworkPlayer _player;

        [ClientRpc]
        public void InitializeAndSetupPlayer_ClientRpc(ClientRpcParams clientRpcParams = default)
        {
            _player = _miniGame.InitializePlayer() as NetworkPlayer;
            _player.Setup(NetworkShipClientCache.OwnShip.Ship);
            GameManager.Instance.WaitOnPlayerLoading();
        }
    }
}
