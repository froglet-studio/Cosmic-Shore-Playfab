using CosmicShore.Game.AI;
using UnityEngine;

public class SingleStickShipTransformer : ShipTransformer
{
    Quaternion additionalRotation = Quaternion.identity;
    GameObject courseObject;
    Transform courseTransform;

    protected override void Start()
    {
        base.Start();
        Ship.ShipStatus.SingleStickControls = true;
        GetComponent<AIPilot>().SingleStickControls = true;


        courseObject = new GameObject("CourseObject");
        courseTransform = courseObject.transform;
    }

    protected override void Pitch() // These need to not use *= because quaternions are not commutative
    {
        accumulatedRotation = Quaternion.AngleAxis(
                            -inputStatus.EasedLeftJoystickPosition.y * (shipStatus.Speed * RotationThrottleScaler + PitchScaler) * Time.deltaTime,
                            courseTransform.right) * accumulatedRotation;
        //additionalRotation = Quaternion.AngleAxis(
        //                    -inputController.EasedRightJoystickPosition.y * lookScalar,
        //                    courseTransform.right) * additionalRotation;
    }

    protected override void Yaw()
    {
        accumulatedRotation = Quaternion.AngleAxis(
                            inputStatus.EasedLeftJoystickPosition.x * (shipStatus.Speed * RotationThrottleScaler + YawScaler) * Time.deltaTime,
                            courseTransform.up) * accumulatedRotation;
        //additionalRotation = Quaternion.AngleAxis(
        //                    inputController.EasedRightJoystickPosition.x * lookScalar,
        //                    courseTransform.up) * additionalRotation;
    }

    protected override void Roll()
    {
        accumulatedRotation = Quaternion.AngleAxis(
                            -inputStatus.EasedLeftJoystickPosition.x * (shipStatus.Speed * RotationThrottleScaler + RollScaler) * Time.deltaTime, //use roll scaler to adjust the banking into turns
                            transform.forward) * accumulatedRotation;
    }

    protected override void RotateShip()
    {
        if (inputController != null)
        {

            Roll();
            Yaw();
            Pitch();
        }

        courseTransform.rotation = Quaternion.Lerp(courseTransform.rotation, accumulatedRotation, lerpAmount * Time.deltaTime);
        transform.rotation = Quaternion.Lerp(transform.rotation, additionalRotation * accumulatedRotation, Time.deltaTime);

        additionalRotation = Quaternion.identity;

        shipStatus.Course = courseTransform.forward;
    }

    protected override void MoveShip()
    {
        float boostAmount = 1f;
        if (shipStatus.Boosting) // TODO: if we run out of fuel while full speed and straight the ship data still thinks we are boosting
        {
            boostAmount = Ship.BoostMultiplier;
        }
        if (shipStatus.ChargedBoostDischarging) boostAmount *= shipStatus.ChargedBoostCharge;
        if (inputController != null)
            shipStatus.Speed = Mathf.Lerp(shipStatus.Speed, inputStatus.XDiff * ThrottleScaler * boostAmount + MinimumSpeed, lerpAmount * Time.deltaTime);

        shipStatus.Speed *= throttleMultiplier;

        transform.position += (shipStatus.Speed * shipStatus.Course + velocityShift) * Time.deltaTime;
    }



}