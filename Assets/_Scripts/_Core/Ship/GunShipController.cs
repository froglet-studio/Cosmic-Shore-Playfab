using UnityEngine;
using StarWriter.Core;

public class GunShipController : ShipController
{
    [SerializeField] Gun topGun;
    [SerializeField] Gun leftGun;
    [SerializeField] Gun rightGun;
    [SerializeField] TrailFollower trailFollower;

    float chargeDepletionRate = -.05f;
    float rechargeRate = .1f;

    public float ProjectileScale = 1f;
    public Vector3 BlockScale = new Vector3(4f, 4f, 1f);

    public int nextBlockIndex = 1;
    public int previousBlockIndex;
    //float trailLerpAmount;
    bool moveForward = true;
    bool attached = false;

    [SerializeField] float maxTrailSpeed = 1f;
    [SerializeField] float reducedTrailSpeed = 1f;

    //int padding = 3; 

    Player player;

    protected override void Start()
    {
        base.Start();
        player = GameObject.FindWithTag("Player").GetComponent<Player>();

        topGun.Team = player.Team;
        topGun.Ship = ship;

        leftGun.Team = player.Team;
        leftGun.Ship = ship;
        
        rightGun.Team = player.Team;
        rightGun.Ship = ship;
        
    }

    protected override void Update()
    {
        base.Update();
        if (resourceSystem.CurrentAmmo > 0 && shipData.GunsActive) Fire();
    }
    protected override void MoveShip()
    {
        if (shipData.Attached && !attached)
        {
            attached = shipData.Attached;
            trailFollower.Attach(shipData.AttachedTrailBlock);
        }
        else if (!shipData.Attached && attached)
        {
            attached = shipData.Attached;
            trailFollower.Detach();
        }

        if (attached)
        {
            Slide();
        }
        else
        {
            base.MoveShip();
        }
    }

    public void BigFire()
    {
        if (resourceSystem.CurrentAmmo > resourceSystem.MaxAmmo / 10f)
        {
            resourceSystem.ChangeAmmoAmount(uuid, -resourceSystem.MaxAmmo / 10f);
            topGun.FireGun(player.transform, 20, shipData.Course * shipData.Speed, ProjectileScale * 15, BlockScale * 2, true, 4f);
        }
        
    }

    void Fire()
    {
        resourceSystem.ChangeAmmoAmount(uuid, chargeDepletionRate * Time.deltaTime); // TODO: this should probably be an amount not a rate. let the gun cooldown handle delta time, but then there is asymmetry with the recharge rate . . . 
        topGun.FireGun(player.transform, 10, shipData.Course * shipData.Speed, ProjectileScale, BlockScale);
        leftGun.FireGun(player.transform, 10, shipData.Course * shipData.Speed, ProjectileScale, BlockScale);
        rightGun.FireGun(player.transform, 10, shipData.Course * shipData.Speed, ProjectileScale, BlockScale);
    }

    void Slide()
    {

        float lookThreshold = -.6f;
        float throttle;
        float zeroPosition = .2f;

        throttle = (inputController.XDiff - zeroPosition)/(1 - zeroPosition);

        if (Vector3.Dot(transform.forward, shipData.Course) < lookThreshold && throttle > 0)
             moveForward = !moveForward;

        if ((moveForward && throttle > 0) || (!moveForward && throttle < 0))
            trailFollower.SetDirection(TrailFollowerDirection.Forward);
        else
            trailFollower.SetDirection(TrailFollowerDirection.Backward);
            
        resourceSystem.ChangeAmmoAmount(uuid, rechargeRate * Time.deltaTime);
        trailFollower.Throttle = Mathf.Abs(throttle);
        trailFollower.Move();

        shipData.AttachedTrailBlock = trailFollower.AttachedTrailBlock;

        if (shipData.AttachedTrailBlock.destroyed)
        {
            shipData.AttachedTrailBlock.Restore();  
        }
        
        else shipData.AttachedTrailBlock.Grow(4);

        shipData.AttachedTrailBlock.Steal(player.PlayerName, player.Team);

        

    }
}
