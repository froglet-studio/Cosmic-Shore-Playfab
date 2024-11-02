using Unity.Netcode;


namespace CosmicShore.Game.GameplayObjects
{
    public class PersistentPlayer : NetworkBehaviour
    {
        public override void OnNetworkSpawn()
        {
            gameObject.name = "PersistentPlayer_" + OwnerClientId;
        }
    }
}
