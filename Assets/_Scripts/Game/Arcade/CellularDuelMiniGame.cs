using UnityEngine;


namespace CosmicShore.Game.Arcade
{
    public class CellularDuelMiniGame : MiniGame 
    {
        IPlayer _hostilePilot;

        protected override void Start()
        {
            base.Start();
            _hostilePilot.Ship.AIPilot.SkillLevel = .4f + IntensityLevel*.15f;
        }

        public void SetHostilePilot(IPlayer hostilePilot) => _hostilePilot = hostilePilot;
    }
}