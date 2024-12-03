using UnityEngine;
using CosmicShore.Core;
using CosmicShore.Game.IO;
using CosmicShore.Game.UI;


namespace CosmicShore.Game
{
    public class Player : MonoBehaviour, IPlayer
    {
        [SerializeField] public ShipTypes defaultShip = ShipTypes.Dolphin;  // TODO: combine this and DefaultShipType
        [SerializeField] GameObject shipContainer;
        [SerializeField] bool UseHangarConfiguration = true;
        [SerializeField] bool IsAI = false;
        [SerializeField] string _playerName;

        public ShipTypes DefaultShipType { get; private set; }
        public Teams Team { get; private set; }
        public string Name { get; private set; }
        public string PlayerName => _playerName;
        public string PlayerUUID { get; private set; }
        public IShip Ship { get; private set; }
        public bool IsActive { get; private set; }

        GameCanvas _gameCanvas;
        public GameCanvas GameCanvas => 
            _gameCanvas != null ? _gameCanvas : FindObjectOfType<GameCanvas>();

        InputController _inputController;
        public InputController InputController => 
            _inputController != null ? _inputController : GetComponent<InputController>();

        public Transform Transform => transform;

        protected GameManager gameManager;

        protected virtual void Awake()
        {
            gameManager = GameManager.Instance;
        }

        public void Initialize(IPlayer.InitializeData data)
        {
            gameManager = GameManager.Instance;
            DefaultShipType = data.DefaultShipType;
            Team = data.Team;
            _playerName = data.PlayerName;
            PlayerUUID = data.PlayerUUID;
            Name = data.PlayerName;

            Setup();
        }

        public void Setup()
        {
            if (UseHangarConfiguration)
            {
                switch (PlayerName)
                {
                    case "HostileOne":
                        SetupAIShip(Hangar.Instance.LoadHostileAI1Ship(Team));
                        break;
                    case "HostileTwo":
                        SetupAIShip(Hangar.Instance.LoadHostileAI2Ship());
                        break;
                    case "FriendlyOne":
                        SetupAIShip(Hangar.Instance.LoadFriendlyAIShip());
                        break;
                    case "SquadMateOne":
                        SetupAIShip(Hangar.Instance.LoadSquadMateOne());
                        break;
                    case "SquadMateTwo":
                        SetupAIShip(Hangar.Instance.LoadSquadMateTwo());
                        break;
                    case "HostileManta":
                        SetupAIShip(Hangar.Instance.LoadHostileManta());
                        break;
                    case "PlayerOne":
                    case "PlayerTwo":
                    case "PlayerThree":
                    case "PlayerFour":
                    default: // Default will be the players Playfab username
                        Debug.Log($"Player.Start - Instantiate Ship: {PlayerName}");

                        SetupPlayerShip(Hangar.Instance.LoadPlayerShip(DefaultShipType, Team));
                        break;
                }
            }
            else
            {
                if (IsAI)
                    SetupAIShip(Hangar.Instance.LoadShip(defaultShip, Team));
                else
                {
                    SetupPlayerShip(Hangar.Instance.LoadPlayerShip());
                }
            }
        }

        public void SetDefaultShipType(ShipTypes shipType) => DefaultShipType = shipType;

        public void ToggleGameObject(bool toggle) => gameObject.SetActive(toggle);


        protected virtual void SetupPlayerShip(IShip ship)
        {
            Ship = ship;
            Ship.Transform.SetParent(shipContainer.transform, false);
            Ship.AIPilot.enabled = false;

            GetComponent<InputController>().Ship = ship;
            GameCanvas.MiniGameHUD.Ship = ship;

            Ship.Initialize(this, Team);

            gameManager.WaitOnPlayerLoading();
        }

        void SetupAIShip(IShip ship)
        {
            Debug.Log($"Player - SetupAIShip - playerName: {PlayerName}");

            Ship = ship;
            Ship.AIPilot.enabled = true;

            var inputController = GetComponent<InputController>();
            inputController.Ship = Ship;
            Ship.Initialize(this, Team);

            gameManager.WaitOnAILoading(Ship.AIPilot);
        }

        public void ToggleActive(bool active) => IsActive = active;
    }
}
