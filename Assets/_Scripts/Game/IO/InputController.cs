using System.Collections;
using UnityEngine;
using CosmicShore.Core;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;
using CosmicShore.Game.UI;
using CosmicShore.App.Systems;
using UnityEngine.InputSystem.Controls;
using Gyroscope = UnityEngine.InputSystem.Gyroscope;
using Unity.Netcode;
using CosmicShore.Utility.ClassExtensions;

namespace CosmicShore.Game.IO
{
    public class InputController : MonoBehaviour
    {
        struct JoystickData
        {
            public Vector2 joystickStart;
            public int touchIndex;
            public Vector2 joystickNormalizedOffset;
            public Vector2 clampedPosition;
        }

        [SerializeField] GameCanvas gameCanvas;
        [SerializeField] public bool Portrait;
        [SerializeField] bool requireTriggerForThrottle;

        private IInputStatus _inputStatus;
        public IInputStatus InputStatus => _inputStatus ??= TryAddInputStatus();

        public IShip Ship { get; set; }
        [HideInInspector] public bool AutoPilotEnabled;
        [HideInInspector] public static ScreenOrientation currentOrientation;

        const float PHONE_FLIP_THRESHOLD = 0.1f;
        const float PI_OVER_FOUR = 0.785f;
        const float MAP_SCALE_X = 2f;
        const float MAP_SCALE_Y = 2f;
        const float GYRO_INITIALIZATION_RANGE = 0.05f;

        bool phoneFlipState;
        bool leftStickEffectsStarted;
        bool rightStickEffectsStarted;
        bool fullSpeedStraightEffectsStarted;
        bool minimumSpeedStraightEffectsStarted;
        int leftTouchIndex, rightTouchIndex;

        private Vector2 RightJoystickValue, LeftJoystickValue;
        public Vector2 SingleTouchValue;
        public Vector3 ThreeDPosition { get; private set; }
        public bool HasThrottleInput { get; private set; }

        float JoystickRadius;
        Gyroscope gyro;
        Quaternion derivedCorrection;
        Quaternion inverseInitialRotation = new(0, 0, 0, 0);

        #region Unity Lifecycle Methods

        private void OnEnable()
        {
            GameSetting.OnChangeInvertYEnabledStatus += OnToggleInvertY;
            GameSetting.OnChangeInvertThrottleEnabledStatus += OnToggleInvertThrottle;
            EnhancedTouchSupport.Enable();
        }

        private void OnDisable()
        {
            GameSetting.OnChangeInvertYEnabledStatus -= OnToggleInvertY;
            GameSetting.OnChangeInvertThrottleEnabledStatus -= OnToggleInvertThrottle;
            EnhancedTouchSupport.Disable();
        }

        private void Start()
        {
            gameCanvas = FindObjectOfType<GameCanvas>();
            InitializeJoysticks();

#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS)
            InitializeGyroscope();
#endif

            LoadSettings();
        }

        IInputStatus TryAddInputStatus()
        {
            bool found = TryGetComponent(out NetworkObject _);
            if (found)
                return gameObject.GetOrAdd<NetworkInputStatus>();
            else
                return gameObject.GetOrAdd<InputStatus>();
        }

        private void Update()
        {
            if (PauseSystem.Paused || InputStatus.Paused) return;
            ReceiveInput();
        }

#endregion

        #region Initialization Methods

        private void InitializeJoysticks()
        {
            JoystickRadius = Screen.dpi;
            LeftJoystickValue = InputStatus.LeftClampedPosition = InputStatus.LeftJoystickHome = new Vector2(JoystickRadius, JoystickRadius);
            RightJoystickValue = InputStatus.RightClampedPosition = InputStatus.RightJoystickHome = new Vector2(Screen.currentResolution.width - JoystickRadius, JoystickRadius);
        }

        private void InitializeGyroscope()
        {
            InputSystem.EnableDevice(Gyroscope.current);
            gyro = Gyroscope.current;
            StartCoroutine(GyroInitializationCoroutine());
        }

        private void LoadSettings()
        {
            InputStatus.InvertYEnabled = GameSetting.Instance.InvertYEnabled;
            InputStatus.InvertThrottleEnabled = GameSetting.Instance.InvertThrottleEnabled;
        }

        #endregion

        #region Input Processing Methods

        private void ReceiveInput()
        {
            if (AutoPilotEnabled)
            {
                ProcessAutoPilotInput();
            }
            else if (Gamepad.current != null)
            {
                ProcessGamepadInput();
            }
            else
            {
                ProcessTouchInput();
            }
        }

        private void ProcessAutoPilotInput()
        {
            if (Ship.ShipStatus.SingleStickControls)
            {
                InputStatus.EasedLeftJoystickPosition = new Vector2(Ship.AIPilot.X, Ship.AIPilot.Y);
            }
            else
            {
                InputStatus.XSum = Ship.AIPilot.XSum;
                InputStatus.YSum = Ship.AIPilot.YSum;
                InputStatus.XDiff = Ship.AIPilot.XDiff;
                InputStatus.YDiff = Ship.AIPilot.YDiff;

                Debug.Log("Autopilot XSum set: " + InputStatus.XSum);
            }
            PerformSpeedAndDirectionalEffects();
        }

        private void ProcessGamepadInput()
        {
            InputStatus.LeftNormalizedJoystickPosition = Gamepad.current.leftStick.ReadValue();
            InputStatus.RightNormalizedJoystickPosition = Gamepad.current.rightStick.ReadValue();

            if (requireTriggerForThrottle)
                HasThrottleInput = Gamepad.current.rightTrigger.ReadValue() > 0;
            else
                HasThrottleInput = true;

            Debug.Log(InputStatus.LeftNormalizedJoystickPosition);
            Debug.Log(InputStatus.RightNormalizedJoystickPosition);

            Reparameterize();
            ProcessGamePadButtons();
            PerformSpeedAndDirectionalEffects();
        }

        private void ProcessTouchInput()
        {
            HandlePhoneOrientation();
            HandleMultiTouch();
            HandleSingleTouch();

            if (Touch.activeTouches.Count > 0)
            {
                HasThrottleInput = true;
                Reparameterize();
                PerformSpeedAndDirectionalEffects();
                HandleIdleState(false);
            }
            else
            {
                HasThrottleInput = false;
                ResetInputValues();
                HandleIdleState(true);
            }
        }

        private void HandlePhoneOrientation()
        {
            if (Portrait)
            {
                Ship.SetShipUp(90);
            }
            else if (Mathf.Abs(Input.acceleration.y) >= PHONE_FLIP_THRESHOLD)
            {
                UpdatePhoneFlipState();
            }
        }

        private void UpdatePhoneFlipState()
        {
            bool newFlipState = Input.acceleration.y > 0;
            if (newFlipState != phoneFlipState)
            {
                phoneFlipState = newFlipState;
                if (phoneFlipState)
                {
                    Ship.PerformShipControllerActions(InputEvents.FlipAction);
                    currentOrientation = ScreenOrientation.LandscapeRight;
                }
                else
                {
                    Ship.StopShipControllerActions(InputEvents.FlipAction);
                    currentOrientation = ScreenOrientation.LandscapeLeft;
                }
                Debug.Log($"Phone flip state change detected - new flip state: {phoneFlipState}, acceleration.y: {Input.acceleration.y}");
            }
        }

        private void HandleMultiTouch()
        {
            if (Touch.activeTouches.Count >= 2)
            {
                AssignTouchIndices();
                UpdateJoystickValues();

                HandleLeftJoystick();
                HandleRightJoystick();
                // HandleJoystick(ref _inputStatus.LeftJoystickStart, leftTouchIndex, ref _inputStatus.LeftNormalizedJoystickPosition, ref _inputStatus.LeftClampedPosition);
                // HandleJoystick(ref _inputStatus.RightJoystickStart, rightTouchIndex, ref _inputStatus.RightNormalizedJoystickPosition, ref _inputStatus.RightClampedPosition);
                StopStickEffects();
            }
        }

        private void AssignTouchIndices()
        {
            if (Touch.activeTouches.Count == 2)
            {
                if (Touch.activeTouches[0].screenPosition.x <= Touch.activeTouches[1].screenPosition.x)
                {
                    leftTouchIndex = 0;
                    rightTouchIndex = 1;
                }
                else
                {
                    leftTouchIndex = 1;
                    rightTouchIndex = 0;
                }
            }
            else
            {
                leftTouchIndex = GetClosestTouch(LeftJoystickValue);
                rightTouchIndex = GetClosestTouch(RightJoystickValue);
            }
        }

        private void UpdateJoystickValues()
        {
            LeftJoystickValue = Touch.activeTouches[leftTouchIndex].screenPosition;
            RightJoystickValue = Touch.activeTouches[rightTouchIndex].screenPosition;
        }

        private void StopStickEffects()
        {
            if (leftStickEffectsStarted)
            {
                leftStickEffectsStarted = false;
                Ship.StopShipControllerActions(InputEvents.LeftStickAction);
            }
            if (rightStickEffectsStarted)
            {
                rightStickEffectsStarted = false;
                Ship.StopShipControllerActions(InputEvents.RightStickAction);
            }
        }

        private void HandleSingleTouch()
        {
            if (Touch.activeTouches.Count == 1)
            {
                var position = Touch.activeTouches[0].screenPosition;
                if (Ship != null && Ship.ShipStatus.CommandStickControls)
                {
                    ProcessCommandStickControls(position);
                }
                ProcessSingleTouchJoystick(position);
            }
        }

        private void ProcessCommandStickControls(Vector2 position)
        {
            SingleTouchValue = position;
            var tempThreeDPosition = new Vector3((SingleTouchValue.x - Screen.width / 2) * MAP_SCALE_X, (SingleTouchValue.y - Screen.height / 2) * MAP_SCALE_Y, 0);

            if (tempThreeDPosition.sqrMagnitude < 10000 && Touch.activeTouches[0].phase == TouchPhase.Began)
            {
                Ship.PerformShipControllerActions(InputEvents.NodeTapAction);
            }
            else if ((tempThreeDPosition - Ship.Transform.position).sqrMagnitude < 10000 && Touch.activeTouches[0].phase == TouchPhase.Began)
            {
                Ship.PerformShipControllerActions(InputEvents.SelfTapAction);
            }
            else
            {
                ThreeDPosition = tempThreeDPosition;
            }
        }

        private void ProcessSingleTouchJoystick(Vector2 position)
        {
            if (Vector2.Distance(LeftJoystickValue, position) < Vector2.Distance(RightJoystickValue, position))
            {
                HandleLeftJoystick(position);
            }
            else
            {
                HandleRightJoystick(position);
            }
        }

        private void HandleLeftJoystick(Vector2 position)
        {
            if (!leftStickEffectsStarted)
            {
                leftStickEffectsStarted = true;
                Ship.PerformShipControllerActions(InputEvents.LeftStickAction);
            }
            LeftJoystickValue = position;
            leftTouchIndex = 0;
            InputStatus.OneTouchLeft = true;

            HandleLeftJoystick();
            // HandleJoystick(ref _inputStatus.LeftJoystickStart, leftTouchIndex, ref _inputStatus.LeftNormalizedJoystickPosition, ref _inputStatus.LeftClampedPosition);
            InputStatus.RightNormalizedJoystickPosition = Vector3.Lerp(InputStatus.RightNormalizedJoystickPosition, Vector3.zero, 7 * Time.deltaTime);
        }

        private void HandleRightJoystick(Vector2 position)
        {
            if (!rightStickEffectsStarted)
            {
                rightStickEffectsStarted = true;
                if (Ship != null)
                    Ship.PerformShipControllerActions(InputEvents.RightStickAction);
            }
            RightJoystickValue = position;
            rightTouchIndex = 0;
            InputStatus.OneTouchLeft = false;

            HandleRightJoystick();
            // HandleJoystick(ref _inputStatus.RightJoystickStart, rightTouchIndex, ref _inputStatus.RightNormalizedJoystickPosition, ref _inputStatus.RightClampedPosition);
            InputStatus.LeftNormalizedJoystickPosition = Vector3.Lerp(InputStatus.LeftNormalizedJoystickPosition, Vector3.zero, 7 * Time.deltaTime);
        }

        void HandleLeftJoystick()
        {
            CollectDataToHandleLeftJoystick(out JoystickData data);
            HandleJoystick(ref data);
            SaveDataAfterHandleLeftJoystick(in data);
        }

        void CollectDataToHandleLeftJoystick(out JoystickData data)
        {
            data = new()
            {
                joystickStart = InputStatus.LeftJoystickStart,
                touchIndex = leftTouchIndex,
                joystickNormalizedOffset = InputStatus.LeftNormalizedJoystickPosition,
                clampedPosition = InputStatus.LeftClampedPosition,
            };
        }

        void SaveDataAfterHandleLeftJoystick(in JoystickData data)
        {
            InputStatus.LeftJoystickStart = data.joystickStart;
            InputStatus.LeftNormalizedJoystickPosition = data.joystickNormalizedOffset;
            InputStatus.LeftClampedPosition = data.clampedPosition;
        }

        void HandleRightJoystick()
        {
            CollectDataToHandleRightJoystick(out JoystickData data);
            HandleJoystick(ref data);
            SaveDataAfterHandleRightJoystick(in data);
        }

        void CollectDataToHandleRightJoystick(out JoystickData data)
        {
            data = new()
            {
                joystickStart = InputStatus.RightJoystickStart,
                touchIndex = rightTouchIndex,
                joystickNormalizedOffset = InputStatus.RightNormalizedJoystickPosition,
                clampedPosition = InputStatus.RightClampedPosition,
            };
        }

        void SaveDataAfterHandleRightJoystick(in JoystickData data)
        {
            InputStatus.RightJoystickStart = data.joystickStart;
            InputStatus.RightNormalizedJoystickPosition = data.joystickNormalizedOffset;
            InputStatus.RightClampedPosition = data.clampedPosition;
        }

        private void ResetInputValues()
        {
            InputStatus.XSum = 0;
            InputStatus.YSum = 0;
            InputStatus.XDiff = 0;
            InputStatus.YDiff = 0;

            Debug.Log("Reset XSum: " + InputStatus.XSum);
        }

        private void HandleIdleState(bool isIdle)
        {
            if (isIdle != InputStatus.Idle)
            {
                InputStatus.Idle = isIdle;
                if (InputStatus.Idle)
                {
                    Ship?.PerformShipControllerActions(InputEvents.IdleAction);
                }
                else
                {
                    Ship.StopShipControllerActions(InputEvents.IdleAction);
                }
            }
        }

        #endregion

        #region Helper Methods

        void HandleJoystick(ref JoystickData data)
        {
            Touch touch = Touch.activeTouches[data.touchIndex];

            if (touch.phase == TouchPhase.Began || data.joystickStart == Vector2.zero)
                data.joystickStart = touch.screenPosition;

            Vector2 offset = touch.screenPosition - data.joystickStart;
            Vector2 clampedOffset = Vector2.ClampMagnitude(offset, JoystickRadius);
            data.clampedPosition = data.joystickStart + clampedOffset;
            Vector2 normalizedOffset = clampedOffset / JoystickRadius;
            data.joystickNormalizedOffset = normalizedOffset;
        }

        /*private void HandleJoystick(ref Vector2 joystickStart, int touchIndex, ref Vector2 joystick, ref Vector2 clampedPosition)
        {
            Touch touch = Touch.activeTouches[touchIndex];

            if (touch.phase == TouchPhase.Began || joystickStart == Vector2.zero)
                joystickStart = touch.screenPosition;

            Vector2 offset = touch.screenPosition - joystickStart;
            Vector2 clampedOffset = Vector2.ClampMagnitude(offset, JoystickRadius);
            clampedPosition = joystickStart + clampedOffset;
            Vector2 normalizedOffset = clampedOffset / JoystickRadius;
            joystick = normalizedOffset;
        }*/

        private void Reparameterize()
        {
            InputStatus.EasedRightJoystickPosition = new Vector2(Ease(2 * InputStatus.RightNormalizedJoystickPosition.x), Ease(2 * InputStatus.RightNormalizedJoystickPosition.y));
            InputStatus.EasedLeftJoystickPosition = new Vector2(Ease(2 * InputStatus.LeftNormalizedJoystickPosition.x), Ease(2 * InputStatus.LeftNormalizedJoystickPosition.y));

            InputStatus.XSum = Ease(InputStatus.RightNormalizedJoystickPosition.x + InputStatus.LeftNormalizedJoystickPosition.x);
            InputStatus.YSum = -Ease(InputStatus.RightNormalizedJoystickPosition.y + InputStatus.LeftNormalizedJoystickPosition.y);
            InputStatus.XDiff = (InputStatus.RightNormalizedJoystickPosition.x - InputStatus.LeftNormalizedJoystickPosition.x + 2) / 4;
            InputStatus.YDiff = Ease(InputStatus.RightNormalizedJoystickPosition.y - InputStatus.LeftNormalizedJoystickPosition.y);

            Debug.Log("Reparameterize XSum set: " + InputStatus.XSum);

            if (InputStatus.InvertYEnabled)
                InputStatus.YSum *= -1;
            if (InputStatus.InvertThrottleEnabled)
                InputStatus.YDiff = 1 - InputStatus.YDiff;
        }

        private float Ease(float input)
        {
            return input < 0 ? (Mathf.Cos(input * PI_OVER_FOUR) - 1) : -(Mathf.Cos(input * PI_OVER_FOUR) - 1);
        }

        private void PerformSpeedAndDirectionalEffects()
        {
            float threshold = .3f;
            float sumOfRotations = Mathf.Abs(InputStatus.YDiff) + Mathf.Abs(InputStatus.YSum) + Mathf.Abs(InputStatus.XSum);
            float DeviationFromFullSpeedStraight = (1 - InputStatus.XDiff) + sumOfRotations;
            float DeviationFromMinimumSpeedStraight = InputStatus.XDiff + sumOfRotations;

            HandleFullSpeedStraight(DeviationFromFullSpeedStraight, threshold);
            HandleMinimumSpeedStraight(DeviationFromMinimumSpeedStraight, threshold);
        }

        private void HandleFullSpeedStraight(float deviation, float threshold)
        {
            if (deviation < threshold && !fullSpeedStraightEffectsStarted)
            {
                fullSpeedStraightEffectsStarted = true;
                Ship.PerformShipControllerActions(InputEvents.FullSpeedStraightAction);
            }
            else if (fullSpeedStraightEffectsStarted && deviation > threshold)
            {
                fullSpeedStraightEffectsStarted = false;
                Ship.StopShipControllerActions(InputEvents.FullSpeedStraightAction);
            }
        }

        private void HandleMinimumSpeedStraight(float deviation, float threshold)
        {
            if (deviation < threshold && !minimumSpeedStraightEffectsStarted)
            {
                minimumSpeedStraightEffectsStarted = true;
                Ship.PerformShipControllerActions(InputEvents.MinimumSpeedStraightAction);
            }
            else if (minimumSpeedStraightEffectsStarted && deviation > threshold)
            {
                minimumSpeedStraightEffectsStarted = false;
                Ship.StopShipControllerActions(InputEvents.MinimumSpeedStraightAction);
            }
        }

        private int GetClosestTouch(Vector2 target)
        {
            int touchIndex = 0;
            float minDistance = Mathf.Infinity;

            for (int i = 0; i < Touch.activeTouches.Count; i++)
            {
                float distance = Vector2.Distance(target, Touch.activeTouches[i].screenPosition);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    touchIndex = i;
                }
            }
            return touchIndex;
        }

        #endregion

        #region Gyroscope Methods

        private IEnumerator GyroInitializationCoroutine()
        {
            derivedCorrection = GyroQuaternionToUnityQuaternion(Quaternion.Inverse(new Quaternion(0, .65f, .75f, 0)));
            inverseInitialRotation = Quaternion.identity;

            while (!SystemInfo.supportsGyroscope || UnityEngine.InputSystem.Gyroscope.current == null)
            {
                yield return new WaitForSeconds(0.1f);
            }

            var gyro = Input.gyro;
            var lastAttitude = gyro.attitude;
            yield return new WaitForSeconds(0.1f);

            while (!(1 - Mathf.Abs(Quaternion.Dot(lastAttitude, gyro.attitude)) < GYRO_INITIALIZATION_RANGE))
            {
                lastAttitude = gyro.attitude;
                yield return new WaitForSeconds(0.1f);
            }

            inverseInitialRotation = Quaternion.Inverse(GyroQuaternionToUnityQuaternion(gyro.attitude) * derivedCorrection);
        }

        public Quaternion GetGyroRotation()
        {
            return inverseInitialRotation * GyroQuaternionToUnityQuaternion(Input.gyro.attitude) * derivedCorrection;
        }

        private Quaternion GyroQuaternionToUnityQuaternion(Quaternion q)
        {
            return new Quaternion(q.x, -q.z, q.y, q.w);
        }

        public void OnToggleGyro(bool status)
        {
            Debug.Log($"InputController.OnToggleGyro - status: {status}");
            if (SystemInfo.supportsGyroscope && status)
            {
                inverseInitialRotation = Quaternion.Inverse(GyroQuaternionToUnityQuaternion(Input.gyro.attitude) * derivedCorrection);
            }

            InputStatus.IsGyroEnabled = status;
        }

        #endregion

        #region Gamepad Methods

        private void ProcessGamePadButtons()
        {
            bool inputState = InputStatus.Idle;
            HandleGamepadButton(Gamepad.current.leftShoulder, InputEvents.IdleAction, ref inputState);
            InputStatus.Idle = inputState;

            HandleGamepadButton(Gamepad.current.rightShoulder, InputEvents.FlipAction, ref phoneFlipState);
            HandleGamepadTrigger(Gamepad.current.leftTrigger, InputEvents.LeftStickAction);
            HandleGamepadTrigger(Gamepad.current.rightTrigger, InputEvents.RightStickAction);
            HandleGamepadButton(Gamepad.current.bButton, InputEvents.Button1Action);
            HandleGamepadButton(Gamepad.current.aButton, InputEvents.Button2Action);
            HandleGamepadButton(Gamepad.current.rightStickButton, InputEvents.Button2Action);
            HandleGamepadButton(Gamepad.current.xButton, InputEvents.Button3Action);
        }

        private void HandleGamepadButton(ButtonControl button, InputEvents action, ref bool stateFlag)
        {
            if (button.wasPressedThisFrame)
            {
                stateFlag = !stateFlag;
                if (stateFlag)
                    Ship.PerformShipControllerActions(action);
                else
                    Ship.StopShipControllerActions(action);
            }
        }

        private void HandleGamepadButton(ButtonControl button, InputEvents action)
        {
            if (button.wasPressedThisFrame)
                Ship.PerformShipControllerActions(action);
            if (button.wasReleasedThisFrame)
                Ship.StopShipControllerActions(action);
        }

        private void HandleGamepadTrigger(ButtonControl trigger, InputEvents action)
        {
            if (trigger.wasPressedThisFrame)
                Ship.PerformShipControllerActions(action);
            if (trigger.wasReleasedThisFrame)
                Ship.StopShipControllerActions(action);
        }

        #endregion

        #region Public Methods

        public void Button1Press()
        {
            Ship.PerformShipControllerActions(InputEvents.Button1Action);
        }

        public void Button1Release()
        {
            Ship.StopShipControllerActions(InputEvents.Button1Action);
        }

        public void Button2Press()
        {
            Ship.PerformShipControllerActions(InputEvents.Button2Action);
        }

        public void Button2Release()
        {
            Ship.StopShipControllerActions(InputEvents.Button2Action);
        }

        public void Button3Press()
        {
            Ship.PerformShipControllerActions(InputEvents.Button3Action);
        }

        public void Button3Release()
        {
            Ship.StopShipControllerActions(InputEvents.Button3Action);
        }

        public void SetPortrait(bool portrait)
        {
            Portrait = portrait;
        }

        public static bool UsingGamepad()
        {
            return Gamepad.current != null;
        }

        private void OnToggleInvertY(bool status)
        {
            Debug.Log($"InputController.OnToggleInvertY - status: {status}");
            InputStatus.InvertYEnabled = status;
        }

        private void OnToggleInvertThrottle(bool status)
        {
            Debug.Log($"InputController.OnToggleInvertThrottle - status: {status}");
            InputStatus.InvertThrottleEnabled = status;
        }

        #endregion
    }
}