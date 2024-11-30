using CosmicShore.Core;
using CosmicShore.Game.IO;
using CosmicShore.Game.UI;
using UnityEngine;


namespace CosmicShore.Game
{
    /// <summary>
    /// This player is spawned to each client from server as 
    /// the main multiplayer player prefab instance.
    /// </summary>
    public class NetworkPlayer : MonoBehaviour, IPlayer
    {
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
        public void Setup(IShip ship, bool isOwner) =>
            SetupPlayerShip(Hangar.Instance.LoadPlayerShip(ship, ship.GetShipType, ship.Team) as IShip, isOwner);

        public void SetDefaultShipType(ShipTypes shipType) => _defaultShipType = shipType;

        public void ToggleGameObject(bool toggle) =>
            gameObject.SetActive(toggle);

        private void SetupPlayerShip(IShip ship, bool isOwner)
        {
            _ship = ship;

            if (isOwner)
            {
                // Below logics are for the ship's owner client only.

                GameCanvas = FindObjectOfType<GameCanvas>();
                GameCanvas.MiniGameHUD.Ship = _ship;

                InputController = GetComponent<InputController>();
                InputController.Ship = _ship;
            }

            _ship.AIPilot.enabled = false;
            _ship.Initialize(this, _ship.Team);
        }
    }
}
