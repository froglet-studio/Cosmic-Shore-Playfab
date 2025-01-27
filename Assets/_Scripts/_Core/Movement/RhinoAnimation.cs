using UnityEngine;

class RhinoAnimation : ShipAnimation
{
    [SerializeField] Transform Fusilage;
    [SerializeField] Transform LeftWing;
    [SerializeField] Transform RightWing;
    [SerializeField] Transform LeftEngine;
    [SerializeField] Transform RightEngine;

    [SerializeField] float animationScaler = 25f;
    [SerializeField] float yawAnimationScaler = 80f;

    protected override void PerformShipAnimations(float pitch, float yaw, float roll, float throttle)
    {
        AnimatePart(LeftWing,
                    0,
                    -Brake(throttle) * yawAnimationScaler,
                    (-1+throttle) * yawAnimationScaler);

        AnimatePart(RightWing,
                    0,
                    Brake(throttle) * yawAnimationScaler,
                    (1-throttle) * yawAnimationScaler);

        AnimatePart(Fusilage,
                    pitch * animationScaler,
                    yaw * animationScaler,
                    roll * animationScaler);

        AnimatePart(LeftEngine,
                    0,
                    Brake(throttle) * yawAnimationScaler,
                    -(-1 + throttle) * yawAnimationScaler);

        AnimatePart(RightEngine,
                    0,
                    -Brake(throttle) * yawAnimationScaler,
                    -(1 - throttle) * yawAnimationScaler);
    }

    protected override void AssignTransforms()
    {
        Transforms.Add(LeftWing);
        Transforms.Add(RightWing);
        Transforms.Add(Fusilage);
    }
}