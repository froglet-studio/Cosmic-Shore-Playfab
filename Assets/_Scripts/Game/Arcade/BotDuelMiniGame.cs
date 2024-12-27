using CosmicShore.Game.AI;
using UnityEngine;

namespace CosmicShore.Game.Arcade
{
    public class BotDuelMiniGame : MiniGame 
    {
        IPlayer hostilePilot;
        
        protected override void Start()
        {
            base.Start();
            hostilePilot.Ship.AIPilot.SkillLevel = .4f + IntensityLevel*.15f;
        }

        public void SetHostilePilot(IPlayer pilot) => hostilePilot = pilot;
    }
}