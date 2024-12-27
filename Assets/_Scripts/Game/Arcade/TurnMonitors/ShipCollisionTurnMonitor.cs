using UnityEngine;

namespace CosmicShore.Game.Arcade
{
    public class ShipCollisionTurnMonitor : TurnMonitor
    {
        [SerializeField] int Collisions;
        [SerializeField] MiniGame Game;
        
        [SerializeField] InterfaceReference<IPlayer> hostilePilotReference;
        IPlayer hostilePlayer => hostilePilotReference.Value;

        public override bool CheckForEndOfTurn()
        {
            if (paused) return false;

            if (!StatsManager.Instance.PlayerStats.ContainsKey(Game.ActivePlayer.PlayerName))
                return false;

            return StatsManager.Instance.PlayerStats[Game.ActivePlayer.PlayerName].SkimmerShipCollisions >= Collisions;
        }

        public override void NewTurn(string playerName)
        {
            StatsManager.Instance.ResetStats();


            // TODO: perhaps coerce stats manager to create an entry for the player here
        }
        private void Update()
        {

            if (Display != null)
                Display.text = ((int)((hostilePlayer.Ship.Transform.position - Game.ActivePlayer.Ship.Transform.position).magnitude/10f)).ToString();
        }
    }
}