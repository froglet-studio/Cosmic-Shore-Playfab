using CosmicShore.Core;
using CosmicShore.Game.GameplayObjects;
using CosmicShore.Game.IO;
using CosmicShore.Game.UI;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;


namespace CosmicShore.Game
{
    /// <summary>
    /// This player is spawned to each client from server as 
    /// the main multiplayer player prefab instance.
    /// </summary>
    public class NetworkPlayer : NetworkBehaviour, IPlayer
    {
        public static List<NetworkPlayer> NppList { get; private set; } = new();

        [SerializeField]
        ShipTypes _defaultShipType;

        public ShipTypes DefaultShipType { get => _defaultShipType; }
        public Teams Team { get; private set; }
        public string PlayerName { get; private set; }
        public string PlayerUUID { get; private set; }
        public string Name { get; private set; }
        public InputController InputController { get; private set; }
        public GameCanvas GameCanvas { get; private set; }
        public Transform Transform => transform;
        public bool IsActive { get; private set; } = false;

        IShip _ship;
        public IShip Ship => _ship;

        private void Awake()
        {
            NppList.Clear();
        }

        public override void OnNetworkSpawn()
        {
            NppList.Add(this);

            gameObject.name = "PersistentPlayer_" + OwnerClientId;
        }

        public override void OnNetworkDespawn()
        {
            NppList.Remove(this);
        }

        public void Initialize(IPlayer.InitializeData data)
        {
            _defaultShipType = data.DefaultShipType;
            Team = data.Team;
            PlayerName = data.PlayerName;
            PlayerUUID = data.PlayerUUID;
            Name = data.PlayerName;
        }

        public void ToggleActive(bool active) => IsActive = active;

        /// <summary>
        /// Setup the player
        /// </summary>
        /// <param name="ship"></param>
        /// <param name="isOwner">Is this player owned by this client</param>
        public void Setup(IShip ship)
        {
            _ship = ship;
            _ship = Hangar.Instance.LoadPlayerShip(_ship, _ship.Team, IsOwner);
            _ship.AIPilot.AutoPilotEnabled = false;

            if (IsOwner)
            {
                // Below logics are for the ship's owner client only.

                GameCanvas = FindObjectOfType<GameCanvas>();
                GameCanvas.MiniGameHUD.Ship = _ship;
            }
            bool inputControllerFound = TryGetComponent(out InputController inputController);
            if (inputControllerFound)
            {
                InputController = inputController;
                InputController.enabled = IsOwner;
                if (IsOwner)
                    InputController.Ship = _ship;
            }

            _ship.Initialize(this, _ship.Team);
        }
            

        public void SetDefaultShipType(ShipTypes shipType) => _defaultShipType = shipType;

        public void ToggleGameObject(bool toggle) =>
            gameObject.SetActive(toggle);
    }
}
