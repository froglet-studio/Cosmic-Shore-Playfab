using UnityEngine;
using StarWriter.Core;
using System.Collections.Generic;
using UnityEngine.UIElements;

class UrchinAnimationContoller : ShipAnimation
{

    [SerializeField] Animator animator;

    protected override void Start()
    {
        base.Start();
        
    }

    protected override void Update()
    {
        base.Update();

       
    }

    protected override void PerformShipAnimations(float pitch, float yaw, float roll, float throttle)
    {

        animator.SetFloat("speed", throttle);
    }

    protected override void AssignTransforms()
    {
        throw new System.NotImplementedException();
    }
}