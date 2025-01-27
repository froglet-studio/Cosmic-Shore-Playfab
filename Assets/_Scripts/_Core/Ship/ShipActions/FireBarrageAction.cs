using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarWriter.Core;

public class FireBarrageAction : ShipActionAbstractBase
{
    // TODO: WIP gun firing needs to be reworked
    [SerializeField] GameObject gunContainer;
    List<Gun> guns = new();

    ResourceSystem resourceSystem;
    ShipStatus shipData;
    GameObject projectileContainer;
    [SerializeField] float ammoCost = .03f;
    bool inherit = false;

    public float ProjectileScale = 1f;

    void CopyValues<T>(T from, T to)
    {
        var json = JsonUtility.ToJson(from);
        JsonUtility.FromJsonOverwrite(json, to);
    }

    void Start()
    {
        
        var gunTemplate = gunContainer.GetComponent<Gun>();
        foreach (var child in gunContainer.GetComponentsInChildren<Transform>()) 
        {
            var go = child.gameObject;
            CopyValues<Gun>(gunTemplate, go.AddComponent<Gun>());
            guns.Add(go.GetComponent<Gun>());
            child.LookAt(gunContainer.transform);
            child.Rotate(0, 180, 0);
        }
        projectileContainer = new GameObject($"{ship.Player.PlayerName}_Projectiles");
        shipData = ship.GetComponent<ShipStatus>();
        resourceSystem = ship.ResourceSystem;
    }

    public override void StartAction()
    {
        if (resourceSystem.CurrentAmmo > ammoCost)
        {
            resourceSystem.ChangeAmmoAmount(-ammoCost);

            Vector3 inheritedVelocity;
            

            // TODO: WIP magic numbers
            foreach (var gun in guns)
            {
                if (inherit)
                {
                    if (shipData.Attached) inheritedVelocity = gun.transform.forward;
                    else inheritedVelocity = shipData.Course;
                }
                else inheritedVelocity = Vector3.zero;
                gun.FireGun(projectileContainer.transform, 7, inheritedVelocity * shipData.Speed, ProjectileScale, true, 3f);
            }
        }
    }

    public override void StopAction()
    {

    }


}
