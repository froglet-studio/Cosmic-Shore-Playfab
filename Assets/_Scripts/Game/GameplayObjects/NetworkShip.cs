using CosmicShore.Core;
using Unity.Netcode;
using UnityEngine;


namespace CosmicShore.Game.GameplayObjects
{
    public class NetworkShip : NetworkBehaviour
    {
        [SerializeField]
        Ship _ship;
        public Ship Ship => _ship;
    }

}
