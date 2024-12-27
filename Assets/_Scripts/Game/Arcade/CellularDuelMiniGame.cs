using UnityEngine;


namespace CosmicShore.Game.Arcade
{
    public class CellularDuelMiniGame : MiniGame 
    {
        [SerializeField] InterfaceReference<IPlayer> hostilePilotReference;
        IPlayer _hostilePilot
        {
            get => hostilePilotReference.Value;
            set => hostilePilotReference.Value = value;
        } 

        protected override void Start()
        {
            base.Start();

            IPlayer.InitializeData data = new()
            {
                DefaultShipType = PlayerShipType,
                Team = Teams.Ruby,
                PlayerName = "HostileOne",
                PlayerUUID = "HostileOne",
                Name = "HostileOne"
            };
            _hostilePilot.Initialize(data);
            _hostilePilot.Ship.AIPilot.SkillLevel = .4f + IntensityLevel*.15f;
        }

        public void SetHostilePilot(IPlayer hostilePilot) => _hostilePilot = hostilePilot;
    }
}