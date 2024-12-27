﻿using CosmicShore.Core;
using CosmicShore.Game.IO;
using CosmicShore.Game.UI;


namespace CosmicShore.Game
{
    public interface IPlayer : ITransform
    {
        public ShipTypes DefaultShipType { get; }
        public Teams Team { get; }
        public string PlayerName { get; }
        public string PlayerUUID { get; }
        public string Name { get; }
        public IShip Ship { get; }
        public InputController InputController { get; }
        public GameCanvas GameCanvas { get; }
        public bool IsActive { get; }

        public void Initialize(InitializeData data);
        public void ToggleActive(bool active);
        public void SetDefaultShipType(ShipTypes defaultShipType);
        public void ToggleGameObject(bool toggle);


        public struct InitializeData
        {
            public ShipTypes DefaultShipType;
            public Teams Team;
            public string PlayerName;
            public string PlayerUUID;
            public string Name;
        }
    }
}
