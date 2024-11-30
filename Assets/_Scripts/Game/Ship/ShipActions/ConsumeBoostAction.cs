using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CosmicShore.Core;


public class ConsumeBoostAction : ShipAction
{
    ShipStatus shipData;
    [SerializeField] float boostMultiplier = 4f;
    [SerializeField] float boostDuration = 4f;

    protected override void Start()
    {
        shipData = ship.ShipStatus;
        ship.BoostMultiplier = 0;
    }
    public override void StartAction()
    {
        //if (resourceSystem.CurrentBoost > 0)
        //{         
            StartCoroutine(ConsumeBoostCoroutine());
        //resourceSystem.ChangeBoostAmount(-1);
        //}
        
    }

    public override void StopAction()
    {
        
    }

    IEnumerator ConsumeBoostCoroutine()
    {
        shipData.Boosting = true;
        ship.BoostMultiplier += boostMultiplier;
        yield return new WaitForSeconds(boostDuration);
        ship.BoostMultiplier -= boostMultiplier;
        if (ship.BoostMultiplier <= 0)
        {
            shipData.Boosting = false;
        }
    }
}
