using CosmicShore.Game;
using CosmicShore.Game.Projectiles;
using UnityEngine;

public class DropDecoyAction : ShipAction
{
    [SerializeField] float decoysPerFullAmmo = 3;
    [SerializeField] float dropForwardDistance = 40;
    [SerializeField] float dropRadiusMinRange = 40;
    [SerializeField] float dropRadiusMaxRange = 60;
    [SerializeField] FakeCrystal decoy;

    [SerializeField] int resourceIndex = 0;
    float resourceCost;

    protected override void InitializeShipAttributes()
    {
        base.InitializeShipAttributes();
        resourceCost = resourceSystem.Resources[resourceIndex].MaxAmount / decoysPerFullAmmo;
    }

    public override void StartAction()
    {
        //TODO - need to recreate this method

        /*if (resourceSystem.Resources[resourceIndex].CurrentAmount > resourceCost)
        {
            resourceSystem.ChangeResourceAmount(resourceIndex, -resourceCost);

            var fake = Instantiate(decoy).GetComponent<FakeCrystal>();
            if (Player.ActivePlayer && Player.ActivePlayer.Ship == ship) fake.isplayer = true;
            fake.Team = ship.Team;
            fake.ItemType = ItemType.Debuff;
            fake.transform.position = ship.transform.position;
            fake.transform.position += Quaternion.Euler(0, 0, Random.Range(0, 360)) * ship.transform.up * Random.Range(dropRadiusMinRange, dropRadiusMaxRange);
            fake.transform.position += ship.transform.forward * dropForwardDistance;
        }*/
    }

    public override void StopAction()
    {

    }
}