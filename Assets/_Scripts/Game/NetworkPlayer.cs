using CosmicShore.Core;
using CosmicShore.Game.AI;
using CosmicShore.Game.GameplayObjects;
using CosmicShore.Game.IO;
using CosmicShore.Game.UI;
using UnityEngine;

namespace CosmicShore.Game
{
    public class NetworkPlayer : Player
    {
        protected override void Start()
        {
            // We dont want anything to be called at start
        }

        public void Setup(Ship ship) =>
            SetupPlayerShip(Hangar.Instance.LoadPlayerShip(ship, ship.ShipType, ship.Team));

        public void SetShipParent() =>
            ship.transform.SetParent(shipContainer.transform, false);

        protected override void SetupPlayerShip(Ship shipInstance)
        {
            GameCanvas = FindObjectOfType<GameCanvas>();
            foreach (Transform child in shipContainer.transform) Destroy(child.gameObject);

            ActivePlayer = this;

            shipInstance.GetComponent<AIPilot>().enabled = false;

            ship = shipInstance;
            GetComponent<InputController>().ship = ship;

            GameCanvas.MiniGameHUD.ship = ship;
            ship.Team = Team;
            ship.Player = this;
            ship.Initialize();
        }
    }
}
