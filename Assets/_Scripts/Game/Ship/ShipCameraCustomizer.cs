using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CosmicShore.Core;

namespace CosmicShore
{
    public class ShipCameraCustomizer : MonoBehaviour
    {
        public IShip Ship { get; private set; }

        [SerializeField] public List<ShipCameraOverrides> ControlOverrides;
        [SerializeField] float closeCamDistance;
        [SerializeField] float farCamDistance;
        CameraManager cameraManager;
        public Transform FollowTarget;
        public Vector3 FollowTargetPosition;

        [SerializeField] bool isOrthographic = false;

        public void Initialize(IShip ship)
        {
            Ship = ship;

            cameraManager = Ship.CameraManager;
            cameraManager.isOrthographic = isOrthographic;

            ApplyShipControlOverrides(ControlOverrides);
        }

        void ApplyShipControlOverrides(List<ShipCameraOverrides> controlOverrides)
        {

            // Camera controls are only relevant for human pilots
            if (Ship.AIPilot.AutoPilotEnabled)
                return;

            foreach (ShipCameraOverrides effect in controlOverrides)
            {
                switch (effect)
                {
                    case ShipCameraOverrides.CloseCam:
                        cameraManager.CloseCamDistance = closeCamDistance;
                        break;
                    case ShipCameraOverrides.FarCam:
                        cameraManager.FarCamDistance = farCamDistance;
                        break;
                    case ShipCameraOverrides.SetFollowTarget:
                        FollowTarget.parent = null;
                        FollowTarget.position = FollowTargetPosition;
                        goto case ShipCameraOverrides.ChangeFollowTarget;
                    case ShipCameraOverrides.ChangeFollowTarget:
                        cameraManager.FollowOverride = true;
                        cameraManager.SetupGamePlayCameras();
                        break;
                    
                }
            }
        }
    }
}