using CosmicShore.Core;
using CosmicShore.Game.Projectiles;
using UnityEngine;

public class FireGunAction : ShipAction
{
    // TODO: WIP gun firing needs to be reworked
    [SerializeField] Gun gun;

    ShipStatus shipData;
    [SerializeField] GameObject projectileContainer;
    [SerializeField] float ammoCost = .03f;

    public float ProjectileScale = 1f;
    public int Energy = 0;
    public float Speed = 90;
    public ElementalFloat ProjectileTime = new ElementalFloat(3f);

    [SerializeField] int ammoIndex = 0;

    protected override void Start()
    {
        base.Start();
        projectileContainer.transform.parent = ship.Player.Transform;
        shipData = ship.ShipStatus;
    }
    public override void StartAction()
    {
        if (resourceSystem.Resources[ammoIndex].CurrentAmount >= ammoCost) 
        {
            resourceSystem.ChangeResourceAmount(ammoIndex, - ammoCost);

            Vector3 inheritedVelocity;
            if (shipData.Attached) inheritedVelocity = gun.transform.forward;
            else inheritedVelocity = shipData.Course;

            gun.FireGun(projectileContainer.transform, Speed, inheritedVelocity * shipData.Speed, ProjectileScale, true, ProjectileTime.Value, 0, FiringPatterns.Default, Energy);
        }
    }

    public override void StopAction()
    {

    }
}