using UnityEngine;
using TMPro;
using Cinemachine;

namespace StarWriter.Core.Input
{
    public class TutorialInputController : MonoBehaviour
    {
       

        public delegate void OnThrottle();
        public static event OnThrottle OnThrottleEvent;

        #region Camera 
        [SerializeField]
        CinemachineVirtualCameraBase CloseCam;

        [SerializeField]
        CinemachineVirtualCameraBase FarCam;

        readonly int activePriority = 10;
        readonly int inactivePriority = 1;
        #endregion

        #region Ship
        [SerializeField] 
        Transform shipTransform;

        [SerializeField]
        Transform Fusilage;

        [SerializeField]
        Transform LeftWing;

        [SerializeField]
        Transform RightWing;
        #endregion

        #region UI
        [SerializeField]
        RectTransform IntensityBar;
        #endregion

        [SerializeField]
        float rotationSpeed = 3;

        [SerializeField]
        float rotationThrottleScaler = 3;

        

        [SerializeField]
        float speed = 20;

        [SerializeField]
        float OnThrottleEventThreshold = 1;

        private float throttle;
        private readonly float defaultThrottle = .3f;
        private readonly float lerpAmount = .2f;

        private readonly float touchScaler = .005f;

        private readonly float yawAnimationScale = .04f;
        private readonly float throttleAnimationScale = 80;

        private Gyroscope gyro;
        private Quaternion empiricalCorrection;
        private Quaternion displacementQ;

        public enum ControlScheme
        {
            All,
            Pitch,
            Roll,
            Yaw,
            Throttle,
            Gyro
        }
        public ControlScheme flightControlScheme;


        private void Awake()
        {
            if (SystemInfo.supportsGyroscope)
            {
                gyro = UnityEngine.Input.gyro;
                empiricalCorrection = Quaternion.Inverse(new Quaternion(0, .65f, .75f, 0)); // TODO: move to derivedCoorection
            }
        }

        void Start()
        {
            if (SystemInfo.supportsGyroscope)
            {
                empiricalCorrection = GyroToUnity(empiricalCorrection);
                gyro.enabled = true;
                Screen.sleepTimeout = SleepTimeout.NeverSleep;
                displacementQ = new Quaternion(0, 0, 0, -1);
            }
        }

        void Update()
        {
            // TODO: remove this check once movement is based on time.deltaTime
            if (PauseSystem.GetIsPaused())
            {
                return;
            }

            if (SystemInfo.supportsGyroscope)
            {
                // Updates GameObjects rotation from input device's gyroscope
                shipTransform.rotation = Quaternion.Lerp(
                                            shipTransform.rotation, 
                                            displacementQ * GyroToUnity(gyro.attitude) * empiricalCorrection, 
                                            lerpAmount);
            }

            //change the camera if you flip you phone
            if (UnityEngine.Input.acceleration.y > 0)
            {
                IntensityBar.rotation = Quaternion.Euler(0,0,180);
                CloseCam.Priority = activePriority;
                FarCam.Priority = inactivePriority;
            }
            else
            {
                IntensityBar.rotation = Quaternion.identity;
                FarCam.Priority = activePriority;
                CloseCam.Priority = inactivePriority;
            }

            if (UnityEngine.Input.touches.Length == 2)
            {
                Vector2 leftTouch, rightTouch;

                if (UnityEngine.Input.touches[0].position.x <= UnityEngine.Input.touches[1].position.x)
                {
                    leftTouch = UnityEngine.Input.touches[0].position;
                    rightTouch = UnityEngine.Input.touches[1].position;
                }
                else
                {
                    leftTouch = UnityEngine.Input.touches[1].position;
                    rightTouch = UnityEngine.Input.touches[0].position;
                }
               /* Pitch(leftTouch.y, rightTouch.y);
                Roll(leftTouch.y, rightTouch.y);
                Yaw(leftTouch.x, rightTouch.x);
                Throttle(leftTouch.x, rightTouch.x);*/
                switch (flightControlScheme)
                {
                    case ControlScheme.All:
                        Pitch(leftTouch.y, rightTouch.y);
                        Roll(leftTouch.y, rightTouch.y);
                        Yaw(leftTouch.x, rightTouch.x);
                        Throttle(leftTouch.x, rightTouch.x);
                        break;
                    case ControlScheme.Pitch:
                        Pitch(leftTouch.y, rightTouch.y);
                        Throttle(leftTouch.x, rightTouch.x);
                        
                        break;
                    case ControlScheme.Roll:
                        Roll(leftTouch.y, rightTouch.y);
                        break;
                    case ControlScheme.Yaw:
                        Yaw(leftTouch.x, rightTouch.x);
                        break;
                    case ControlScheme.Throttle:
                        Throttle(leftTouch.x, rightTouch.x);
                        break;
                    case ControlScheme.Gyro:

                        //TODO add in gyro only
                        Throttle(leftTouch.x, rightTouch.x);
                        break;
                }
                PerformShipAnimations(leftTouch.y, rightTouch.y, leftTouch.x, rightTouch.x);
            }
            else
            {
                throttle = Mathf.Lerp(throttle, defaultThrottle, .1f);
                LeftWing.localRotation = Quaternion.Lerp(LeftWing.localRotation, Quaternion.identity, .1f);
                RightWing.localRotation = Quaternion.Lerp(RightWing.localRotation, Quaternion.identity, .1f);
                Fusilage.localRotation = Quaternion.Lerp(Fusilage.localRotation, Quaternion.identity, .1f);
            }

            // Move ship forward
            shipTransform.position += speed * throttle * Time.deltaTime * shipTransform.forward;
        }

        private void PerformShipAnimations(float yl, float yr, float xl, float xr)
        {
            // Ship animations TODO: figure out how to leverage a single definition for pitch, etc. that captures the gyro in the animations.
            LeftWing.localRotation = Quaternion.Lerp(
                                        LeftWing.localRotation, 
                                        Quaternion.Euler(
                                            (-(yl + yr) + (Screen.currentResolution.height) + (yr - yl)) * .02f, //tilt based on pitch and roll
                                            0,
                                            -(throttle - defaultThrottle) * throttleAnimationScale - ((xl + xr) - (Screen.currentResolution.width)) * yawAnimationScale), //sweep back based on throttle and yaw
                                        lerpAmount);

            RightWing.localRotation = Quaternion.Lerp(
                                        RightWing.localRotation, 
                                        Quaternion.Euler(
                                            (-(yl + yr) + Screen.currentResolution.height - (yr - yl)) * .02f, 
                                            0,
                                            (throttle - defaultThrottle) * throttleAnimationScale - ((xl + xr) - Screen.currentResolution.width) * yawAnimationScale), 
                                        lerpAmount);

            Fusilage.localRotation = Quaternion.Lerp(
                                        Fusilage.localRotation, 
                                        Quaternion.Euler(
                                            (-(yl + yr) + Screen.currentResolution.height) * .02f,
                                            (yr - yl)*.02f,
                                            (-(xl + xr) + Screen.currentResolution.width) * .01f),
                                        lerpAmount);
        }

        private void Throttle(float xl, float xr)
        {
            throttle = Mathf.Lerp(throttle, (xr - xl) * touchScaler * .18f - .15f, .2f);

            if (throttle > OnThrottleEventThreshold)
                OnThrottleEvent?.Invoke();
        }
        
        private void Yaw(float xl, float xr)  // These need to not use *= ... remember quaternions are not commutative
        {
            displacementQ = Quaternion.AngleAxis(
                                (((xl + xr) / 2) - (Screen.currentResolution.width / 2)) * touchScaler * (throttle*rotationThrottleScaler+rotationSpeed), 
                                shipTransform.up) * displacementQ;
        }

        private void Roll(float yl, float yr)
        {
            displacementQ = Quaternion.AngleAxis(
                                (yr - yl) * touchScaler, 
                                shipTransform.forward) * displacementQ;
        }

        private void Pitch(float yl, float yr)
        {
            displacementQ = Quaternion.AngleAxis(
                                (((yl + yr) / 2) - (Screen.currentResolution.height / 2)) * -touchScaler * (throttle * rotationThrottleScaler + rotationSpeed), 
                                shipTransform.right) * displacementQ;
        }

        //Converts Android Quaterions into Unity Quaterions
        private Quaternion GyroToUnity(Quaternion q)
        {
            return new Quaternion(q.x, -q.z, q.y, q.w);
        }
    }
}