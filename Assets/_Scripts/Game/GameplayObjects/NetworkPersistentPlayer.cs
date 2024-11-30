using CosmicShore.Game.IO;
using Unity.Netcode;
using UnityEngine;


namespace CosmicShore.Game.GameplayObjects
{
    public class NetworkPersistentPlayer : NetworkBehaviour
    {
        [SerializeField]
        InputController _inputController;

        public override void OnNetworkSpawn()
        {
            gameObject.name = "PersistentPlayer_" + OwnerClientId;

            if (!IsOwner)
            {
                _inputController.enabled = false;
            }
        }
    }
}
