using CosmicShore.Core;
using CosmicShore.Game.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;


namespace CosmicShore.Game.UI
{
    public class ThumbPerimeter : MonoBehaviour
    {
        
        [SerializeField] bool LeftThumb;
        bool PerimeterActive = false;
        [SerializeField] float Scaler = 3f; //scale to max radius

        [SerializeField] Sprite ActivePerimeterImage;
        [SerializeField] IPlayer player;
        public float alpha = 0f;        
        
        Image image;
        Color color = Color.white;

        bool initialized;
        bool imageEnabled = true;

        Vector2 leftStartPosition, rightStartPosition;
        InputController controller;
        IInputStatus inputStatus;
        IPlayer _player;

        private void OnEnable()
        {
            GameSetting.OnChangeJoystickVisualsStatus += OnToggleJoystickVisuals;
        }

        private void OnDisable()
        {
            GameSetting.OnChangeJoystickVisualsStatus -= OnToggleJoystickVisuals;
        }

        private void OnToggleJoystickVisuals(bool status)
        {
            Debug.Log($"GameSettings.OnChangeJoystickVisualsStatus - status: {status}");
            imageEnabled = status;
        }

        void Start()
        {
            image = GetComponent<Image>();
            image.sprite = ActivePerimeterImage;
            imageEnabled = GameSetting.Instance.JoystickVisualsEnabled;
            //set Image color alpha
            color.a = 0;
            image.color = color;
        }

        // TODO - Need to call this from somewhere after player initializes.
        public void Initialize(IPlayer player)
        {
            this.player = player;
            inputStatus = this.player.InputController.InputStatus;
            bool isActive = Gamepad.current == null && !_player.Ship.ShipStatus.CommandStickControls && (LeftThumb || _player.Ship.ShipStatus.SingleStickControls);
            if (!_player.Ship.ShipStatus.AutoPilotEnabled)
            {
                gameObject.SetActive(isActive);
                controller = _player.Ship.InputController;
                initialized = isActive;
            }
        }

        void Update()
        {
            if(!imageEnabled) { return; }

            if (initialized && !_player.Ship.ShipStatus.AutoPilotEnabled)
            {
                if (Input.touches.Length == 0)
                {
                    color.a = 0;
                    image.color = color;
                }

                else
                {
                    float normalizedJoystickDistance;
                    float angle;
                    Vector2 normalizedJoystickPosition;
                    if (Input.touches.Length == 1)
                    {
                        PerimeterActive = inputStatus.OneTouchLeft == LeftThumb;
                    }                  
                    if (LeftThumb)
                    {
                        transform.position = inputStatus.LeftJoystickStart;
                        normalizedJoystickPosition = inputStatus.LeftNormalizedJoystickPosition;
                    }
                    else
                    {
                        transform.position = inputStatus.RightJoystickStart;
                        normalizedJoystickPosition = inputStatus.RightNormalizedJoystickPosition;
                    }
                    normalizedJoystickDistance = normalizedJoystickPosition.magnitude;

                    image.rectTransform.localScale = Vector2.one * Scaler;
                    image.sprite = ActivePerimeterImage;
                    //set Image color alpha
                   
                    color.a = normalizedJoystickDistance - .5f;
                    image.color = color;
                    alpha = color.a;

                    angle = Vector3.Angle(normalizedJoystickPosition, Vector2.up);

                    transform.rotation = Vector2.Dot(normalizedJoystickPosition, Vector2.right) < 0 ?
                        Quaternion.Euler(0, 0, angle) :
                        Quaternion.Euler(0, 0, -angle);   
                }
            }
        }
    }
}