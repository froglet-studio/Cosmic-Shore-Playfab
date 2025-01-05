using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using CosmicShore.Core;


namespace CosmicShore.Game.UI
{
    public class ThumbCursor : MonoBehaviour
    {
        [SerializeField] bool LeftThumb;
        [SerializeField] Vector2 offset;
        [SerializeField] Sprite InactiveImage;
        [SerializeField] Sprite ActiveImage;

        Image image;
        bool initialized;
        bool imageEnabled = true;
        Vector2 leftTouch, rightTouch;
        IPlayer player;
        IInputStatus inputStatus;

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
            image.sprite = InactiveImage;
            imageEnabled = GameSetting.Instance.JoystickVisualsEnabled;
        }

        public void Initialize(IPlayer player)
        {
            this.player = player;
            if (!player.Ship.ShipStatus.AutoPilotEnabled)
                gameObject.SetActive(Gamepad.current == null && !player.Ship.ShipStatus.CommandStickControls && (LeftThumb || !player.Ship.ShipStatus.SingleStickControls));

            initialized = true;
            inputStatus = player.Ship.InputController.InputStatus;
        }

        void Update()
        {
            if (initialized && !player.Ship.ShipStatus.AutoPilotEnabled)
            {
                if (Input.touches.Length == 0)
                {
                    transform.position = LeftThumb ? Vector2.Lerp(transform.position, inputStatus.LeftJoystickHome, .2f) : Vector2.Lerp(transform.position, inputStatus.RightJoystickHome, .2f);
                    image.sprite = InactiveImage;
                }
                else if (LeftThumb)
                {
                    leftTouch = inputStatus.LeftClampedPosition;
                    transform.position = Vector2.Lerp(transform.position, leftTouch, .2f);
                    imageEnabled = true ? image.sprite = ActiveImage : image.sprite = InactiveImage;
                    
                    //image.transform.localScale = (Player.ActivePlayer.Ship.InputController.LeftJoystickStart              //makes circles grow as they get close to perimeter
                    //    - Player.ActivePlayer.Ship.InputController.LeftClampedPosition).magnitude * .025f * Vector3.one;
                }
                else
                {
                    rightTouch = inputStatus.RightClampedPosition;
                    transform.position = Vector2.Lerp(transform.position, rightTouch, .2f);
                    imageEnabled = true ? image.sprite = ActiveImage : image.sprite = InactiveImage;
                    //image.transform.localScale = (Player.ActivePlayer.Ship.InputController.RightJoystickStart
                    //    - Player.ActivePlayer.Ship.InputController.RightClampedPosition).magnitude * .025f * Vector3.one;
                }
            }
        }
    }
}