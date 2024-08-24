using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CosmicShore.Core;


namespace CosmicShore
{
    public class ClearPrisms : MonoBehaviour
    {
        Transform mainCamera;
        [SerializeField] Ship Ship;
        public float capsuleRadius = 5f;
        Transform visibilityCapsuleTransform;

        private CapsuleCollider visibilityCapsule;
        private Dictionary<Renderer, Material> originalMaterials = new Dictionary<Renderer, Material>();

        CameraManager cameraManager;

        void Start()
        {
            cameraManager = Ship.cameraManager;
            mainCamera = cameraManager.GetCloseCamera();
            visibilityCapsuleTransform = new GameObject("Visibility Capsule").transform;
            transform.SetParent(visibilityCapsuleTransform);
            visibilityCapsule = gameObject.AddComponent<CapsuleCollider>();
            visibilityCapsule.isTrigger = true;
            visibilityCapsule.radius = capsuleRadius;
        }

        void Update()
        {
            if (mainCamera == null) return;

            Vector3 cameraPosition = mainCamera.position;
            Vector3 shipPosition = Ship.transform.position;

            // Position the capsule between the camera and the ship
            transform.position = (cameraPosition + shipPosition) / 2f;
            transform.LookAt(shipPosition);

            // Scale the capsule to fit between the camera and ship
            float distance = Vector3.Distance(cameraPosition, shipPosition);
            visibilityCapsule.height = distance;

            // Update the capsule's end positions
            Vector3 capsuleDirection = (shipPosition - cameraPosition).normalized;
            visibilityCapsule.center = Vector3.zero;
            transform.up = capsuleDirection;
        }

        void OnTriggerEnter(Collider other)
        {
            TrailBlock trailBlock = other.GetComponent<TrailBlock>();
            if (trailBlock != null)
            {
                Renderer renderer = trailBlock.GetComponent<Renderer>();
                Teams team = trailBlock.Team;
                if (renderer != null && !originalMaterials.ContainsKey(renderer))
                {
                    originalMaterials[renderer] = renderer.material;
                    renderer.material = Hangar.Instance.GetTeamTransparentBlockMaterial(team);
                }
            }
        }

        void OnTriggerExit(Collider other)
        {
            TrailBlock trailBlock = other.GetComponent<TrailBlock>();
            if (trailBlock != null)
            {
                Renderer renderer = trailBlock.GetComponent<Renderer>();
                if (renderer != null && originalMaterials.ContainsKey(renderer))
                {
                    Debug.Log("Clear Prisms: exiting trail block");
                    renderer.material = originalMaterials[renderer];
                    originalMaterials.Remove(renderer);
                }
            }
        }

        void OnDisable()
        {
            // Restore all materials when the script is disabled
            foreach (var kvp in originalMaterials)
            {
                if (kvp.Key != null)
                {
                    kvp.Key.material = kvp.Value;
                }
            }
            originalMaterials.Clear();
        }
    }
}