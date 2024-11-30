using CosmicShore.Core;
using Unity.Netcode;
using UnityEngine;


namespace CosmicShore.Game.GameplayObjects
{
    public class NetworkShip : NetworkBehaviour
    {
        [SerializeField] IShip _ship;
        public IShip Ship => _ship;

        NetworkVariable<float> n_Speed = new NetworkVariable<float>(
            writePerm: NetworkVariableWritePermission.Owner);

        public override void OnNetworkSpawn()
        {
            if (!IsOwner)
                n_Speed.OnValueChanged += OnSpeedValueChanged;
        }

        private void Update()
        {
            if (IsOwner)
            {
                if (_ship == null || _ship.ShipStatus == null)
                    return;

                n_Speed.Value = _ship.ShipStatus.Speed;
            }
        }

        public override void OnNetworkDespawn()
        {
            if (!IsOwner)
                n_Speed.OnValueChanged -= OnSpeedValueChanged;
        }

        private void OnSpeedValueChanged(float previousValue, float newValue)
        {
            _ship.ShipStatus.Speed = newValue;
        }
    }

}
