using System.Collections.Generic;
using StarWriter.Core.Input;
using UnityEngine;
using UnityEngine.Serialization;

namespace StarWriter.Core
{
    [RequireComponent(typeof(ResourceSystem))]
    [RequireComponent(typeof(TrailSpawner))]
    public class Ship : MonoBehaviour
    {
        [Header("Ship Meta")]
        [SerializeField] string Name;
        [SerializeField] public ShipTypes ShipType;

        [Header("Ship Components")]
        [SerializeField] Skimmer nearFieldSkimmer;
        [SerializeField] Skimmer farFieldSkimmer;
        [SerializeField] GameObject OrientationHandle;
        [SerializeField] GameObject AOEPrefab;
        [SerializeField] List<GameObject> shipGeometries;
        [HideInInspector] public TrailSpawner TrailSpawner;
        [SerializeField] GameObject head;

        [Header("Environment Interactions")]
        [SerializeField] List<CrystalImpactEffects> crystalImpactEffects;
        [SerializeField] List<TrailBlockImpactEffects> trailBlockImpactEffects;

        [Header("Configuration")]
        public float boostMultiplier = 4f;
        public float boostFuelAmount = -.01f;
        [SerializeField] float rotationScaler = 130;
        [SerializeField] float rotationThrottleScaler;
        [SerializeField] float minExplosionScale = 50;
        [SerializeField] float maxExplosionScale = 400;

        [SerializeField] float blockFuelChange;
        [SerializeField] float closeCamDistance;
        [SerializeField] float farCamDistance;
        [SerializeField] float minFarFieldSkimmerScale = 100;
        [SerializeField] float maxFarFieldSkimmerScale = 200;
        [SerializeField] float minNearFieldSkimmerScale = 15;
        [SerializeField] float maxNearFieldSkimmerScale = 100;
        [SerializeField] bool boostSkimmerScaling = false;
        [SerializeField] float BoostSkimmerScaler = .01f;

        [Header("Dynamically Assignable Controls")]
        [SerializeField] List<ShipActions> fullSpeedStraightEffects;
        [SerializeField] List<ShipActions> rightStickEffects;
        [SerializeField] List<ShipActions> leftStickEffects;
        [SerializeField] List<ShipActions> flipEffects;
        [SerializeField] List<ShipControlOverrides> controlOverrides;

        Dictionary<ShipControls, List<ShipActions>> ShipControlActions;

        bool invulnerable;
        Teams team;
        CameraManager cameraManager;
        Player player;
        ShipData shipData; // TODO: this should be a required component or just a series of properties on the ship
        InputController inputController;
        Material ShipMaterial;
        Material AOEExplosionMaterial;
        ResourceSystem resourceSystem;
        readonly List<ShipSpeedModifier> SpeedModifiers = new List<ShipSpeedModifier>();
        float speedModifierDuration = 2f;
        float speedModifierMax = 6f;
        float abilityStartTime;
        float boostDuration;
        bool boostSkimmerScalingStopped;

        public Teams Team 
        { 
            get => team; 
            set 
            { 
                team = value;
                if (nearFieldSkimmer != null) nearFieldSkimmer.team = value;
                if (farFieldSkimmer != null) farFieldSkimmer.team = value; 
            }
        }
        public Player Player 
        { 
            get => player;
            set
            {
                player = value;
                if (nearFieldSkimmer != null) nearFieldSkimmer.Player = value;
                if (farFieldSkimmer != null) farFieldSkimmer.Player = value;
            }
        }

        void Start()
        {
            TrailSpawner = GetComponent<TrailSpawner>();
            cameraManager = CameraManager.Instance;
            shipData = GetComponent<ShipData>();
            resourceSystem = GetComponent<ResourceSystem>();
            inputController = player.GetComponent<InputController>();
            ApplyShipControlOverrides(controlOverrides);

            foreach (var shipGeometry in shipGeometries)
                shipGeometry.AddComponent<ShipGeometry>().Ship = this;

            ShipControlActions = new Dictionary<ShipControls, List<ShipActions>> { 
                { ShipControls.FullSpeedStraightAction, fullSpeedStraightEffects },
                { ShipControls.FlipAction, flipEffects },
                { ShipControls.LeftStickAction, leftStickEffects },
                { ShipControls.RightStickAction, rightStickEffects }
            };
        }

        void Update()
        {
            ApplySpeedModifiers();
            ScaleSkimmerDuringBoost(); // TODO: turn this into apply skimmer modifiers
        }

        void ApplyShipControlOverrides(List<ShipControlOverrides> controlOverrides)
        {
            foreach (ShipControlOverrides effect in controlOverrides)
            {
                switch (effect)
                {
                    case ShipControlOverrides.TurnSpeed:
                        inputController.rotationScaler = rotationScaler;
                        break;
                    case ShipControlOverrides.BlockScout:
                        break;
                    case ShipControlOverrides.CloseCam:
                        cameraManager.CloseCamDistance = closeCamDistance;
                        cameraManager.SetCloseCameraDistance(closeCamDistance);
                        break;
                    case ShipControlOverrides.FarCam:
                        cameraManager.FarCamDistance = farCamDistance;
                        cameraManager.SetFarCameraDistance(farCamDistance);
                        break;
                    case ShipControlOverrides.SecondMode:
                        // TODO: ship mode toggling
                        break;
                    case ShipControlOverrides.SpeedBasedTurning:
                        inputController.rotationThrottleScaler = rotationThrottleScaler;
                        break;
                }
            }
        }

        public void PerformCrystalImpactEffects(CrystalProperties crystalProperties)
        {
            if (StatsManager.Instance != null)
                StatsManager.Instance.CrystalCollected(this, crystalProperties);

            foreach (CrystalImpactEffects effect in crystalImpactEffects)
            {
                switch (effect)
                {
                    case CrystalImpactEffects.PlayHaptics:
                        HapticController.PlayCrystalImpactHaptics();
                        break;
                    case CrystalImpactEffects.AreaOfEffectExplosion:
                        var AOEExplosion = Instantiate(AOEPrefab).GetComponent<AOEExplosion>();
                        AOEExplosion.Material = AOEExplosionMaterial;
                        AOEExplosion.Team = team;
                        AOEExplosion.Ship = this;
                        AOEExplosion.SetPositionAndRotation(transform.position, transform.rotation);
                        AOEExplosion.MaxScale =  Mathf.Max(minExplosionScale, resourceSystem.CurrentCharge * maxExplosionScale);

                        if (AOEExplosion is AOEBlockCreation aoeBlockcreation)
                            aoeBlockcreation.SetBlockMaterial(TrailSpawner.GetBlockMaterial());

                        break;
                    case CrystalImpactEffects.IncrementLevel:
                        resourceSystem.ChangeLevel(player.PlayerUUID, ChargeDisplay.OneFuelUnit);
                        ScaleSkimmersWithLevel();
                        break;
                    case CrystalImpactEffects.FillCharge:
                        resourceSystem.ChangeChargeAmount(player.PlayerUUID, crystalProperties.fuelAmount);
                        break;
                    case CrystalImpactEffects.Boost:
                        SpeedModifiers.Add(new ShipSpeedModifier(crystalProperties.speedBuffAmount, 4 * speedModifierDuration, 0));
                        break;
                    case CrystalImpactEffects.DrainCharge:
                        resourceSystem.ChangeChargeAmount(player.PlayerUUID, -resourceSystem.CurrentCharge);
                        break;
                    case CrystalImpactEffects.Score:
                        //if (StatsManager.Instance != null)
                        //    StatsManager.Instance.UpdateScore(player.PlayerUUID, crystalProperties.scoreAmount);
                        // TODO: Remove this impact effect, or re-introduce scoring in a separate game mode
                        break;
                    case CrystalImpactEffects.ResetAggression:
                        if (gameObject.TryGetComponent<AIPilot>(out var aiPilot))
                        {
                            aiPilot.lerp = aiPilot.defaultLerp;
                            aiPilot.throttle = aiPilot.defaultThrottle;
                        }
                        break;
                }
            }
        }

        public void PerformTrailBlockImpactEffects(TrailBlockProperties trailBlockProperties)
        {
            foreach (TrailBlockImpactEffects effect in trailBlockImpactEffects)
            {
                switch (effect)
                {
                    case TrailBlockImpactEffects.PlayHaptics:
                        HapticController.PlayBlockCollisionHaptics();
                        break;
                    case TrailBlockImpactEffects.DrainHalfFuel:
                        resourceSystem.ChangeChargeAmount(player.PlayerUUID, -resourceSystem.CurrentCharge / 2f);
                        break;
                    case TrailBlockImpactEffects.DebuffSpeed:
                        SpeedModifiers.Add(new ShipSpeedModifier(trailBlockProperties.speedDebuffAmount, speedModifierDuration, 0));
                        break;
                    case TrailBlockImpactEffects.DeactivateTrailBlock:
                        break;
                    case TrailBlockImpactEffects.ActivateTrailBlock:
                        break;
                    case TrailBlockImpactEffects.OnlyBuffSpeed:
                        if (trailBlockProperties.speedDebuffAmount > 1) SpeedModifiers.Add(new ShipSpeedModifier(trailBlockProperties.speedDebuffAmount, speedModifierDuration, 0));
                        break;
                    case TrailBlockImpactEffects.ChangeCharge:
                        resourceSystem.ChangeChargeAmount(player.PlayerUUID, blockFuelChange);
                        break;
                    case TrailBlockImpactEffects.DecrementLevel:
                        resourceSystem.ChangeLevel(player.PlayerUUID, -ChargeDisplay.OneFuelUnit);
                        ScaleSkimmersWithLevel();
                        break;
                }
            }
        }

        public void PerformShipControllerActions(ShipControls controlType)
        {
            abilityStartTime = Time.time;
            var shipActions = ShipControlActions[controlType];

            foreach (ShipActions action in shipActions)
            {
                switch (action)
                {
                    case ShipActions.Drift:
                        // TODO: this should call inputController.StartDrift
                        shipData.Drifting = true;
                        cameraManager.ZoomOut();
                        break;
                    case ShipActions.Boost:
                        shipData.Boosting = true;
                        break;
                    case ShipActions.Invulnerability:
                        if (!invulnerable)
                        {
                            invulnerable = true;
                            trailBlockImpactEffects.Remove(TrailBlockImpactEffects.DebuffSpeed);
                            trailBlockImpactEffects.Add(TrailBlockImpactEffects.OnlyBuffSpeed);
                        }
                        head.transform.localScale *= 1.02f; // TODO make this its own ability 
                        break;
                    case ShipActions.ToggleCamera:
                        CameraManager.Instance.ToggleCloseOrFarCamOnPhoneFlip(true);
                        TrailSpawner.ToggleBlockWaitTime(true);
                        break;
                    case ShipActions.ToggleMode:
                        // TODO
                        break;
                    case ShipActions.ToggleGyro:
                        inputController.OnToggleGyro(true);
                        break;
                }
            }
        }

        public void StopShipControllerActions(ShipControls controlType)
        {
            if (StatsManager.Instance != null)
                StatsManager.Instance.AbilityActivated(Team, player.PlayerName, controlType, Time.time-abilityStartTime);

            var shipActions = ShipControlActions[controlType];
            foreach (ShipActions action in shipActions)
            {
                switch (action)
                {
                    case ShipActions.Drift:
                        shipData.Drifting = false;
                        inputController.EndDrift();
                        cameraManager.ResetToNeutral();
                        break;
                    case ShipActions.Boost:
                        shipData.Boosting = false;
                        boostSkimmerScalingStopped = true;
                        break;
                    case ShipActions.Invulnerability:
                        invulnerable = false;
                        trailBlockImpactEffects.Add(TrailBlockImpactEffects.DebuffSpeed);
                        trailBlockImpactEffects.Remove(TrailBlockImpactEffects.OnlyBuffSpeed);
                        head.transform.localScale = Vector3.one;
                        break;
                    case ShipActions.ToggleCamera:
                        CameraManager.Instance.ToggleCloseOrFarCamOnPhoneFlip(false);
                        TrailSpawner.ToggleBlockWaitTime(false);
                        break;
                    case ShipActions.ToggleMode:
                        // TODO
                        break;
                    case ShipActions.ToggleGyro:
                        inputController.OnToggleGyro(false);
                        break;
                }
            }
        }

        public void ToggleCollision(bool enabled)
        {
            foreach (var collider in GetComponentsInChildren<Collider>(true))
                collider.enabled = enabled;
        }

        public void SetShipMaterial(Material material)
        {
            ShipMaterial = material;
            ApplyShipMaterial();
        }

        public void SetBlockMaterial(Material material)
        {
            TrailSpawner.SetBlockMaterial(material);
        }

        public void SetAOEExplosionMaterial(Material material)
        {
            AOEExplosionMaterial = material;
        }

        public void FlipShipUpsideDown()
        {
            OrientationHandle.transform.localRotation = Quaternion.Euler(0, 0, 180);
        }
        public void FlipShipRightsideUp()
        {
            OrientationHandle.transform.localRotation = Quaternion.Euler(0, 0, 0);
        }

        void ApplySpeedModifiers()
        {
            float accumulatedSpeedModification = 1;
            for (int i = SpeedModifiers.Count - 1; i >= 0; i--)
            {
                var modifier = SpeedModifiers[i];
                modifier.elapsedTime += Time.deltaTime;
                SpeedModifiers[i] = modifier;

                if (modifier.elapsedTime >= modifier.duration)
                    SpeedModifiers.RemoveAt(i);
                else
                    accumulatedSpeedModification *= Mathf.Lerp(modifier.initialValue, 1f, modifier.elapsedTime / modifier.duration);
            }

            accumulatedSpeedModification = Mathf.Min(accumulatedSpeedModification, speedModifierMax);
            shipData.SpeedMultiplier = accumulatedSpeedModification;
        }

        void ApplyShipMaterial()
        {
            if (ShipMaterial == null)
                return;

            foreach (var shipGeometry in shipGeometries)
                shipGeometry.GetComponent<MeshRenderer>().material = ShipMaterial;
        }

        void ScaleSkimmersWithLevel()
        {
            nearFieldSkimmer.transform.localScale = Vector3.one * (minNearFieldSkimmerScale + ((resourceSystem.CurrentLevel / resourceSystem.MaxLevel) * (maxNearFieldSkimmerScale - minNearFieldSkimmerScale)));
            farFieldSkimmer.transform.localScale = Vector3.one * (maxFarFieldSkimmerScale - ((resourceSystem.CurrentLevel / resourceSystem.MaxLevel) * (maxFarFieldSkimmerScale - minFarFieldSkimmerScale)));
        }

        void ScaleSkimmerDuringBoost()
        {
            if (boostSkimmerScaling && shipData.Boosting && resourceSystem.CurrentCharge > 0)
            {
                boostDuration += Time.deltaTime;
                nearFieldSkimmer.transform.localScale = Mathf.Min(minNearFieldSkimmerScale + boostDuration * BoostSkimmerScaler, maxNearFieldSkimmerScale) * Vector3.one;
            }
            else if (boostSkimmerScaling && boostSkimmerScalingStopped)
            {
                if (nearFieldSkimmer.transform.localScale.z > minNearFieldSkimmerScale)
                {
                    boostDuration -= Time.deltaTime*2;
                    nearFieldSkimmer.transform.localScale = Mathf.Min(minNearFieldSkimmerScale + boostDuration * BoostSkimmerScaler, maxNearFieldSkimmerScale) * Vector3.one;
                }
                else
                {
                    boostSkimmerScalingStopped = false;
                    boostDuration = 0;
                    nearFieldSkimmer.transform.localScale = minNearFieldSkimmerScale * Vector3.one;
                }
                
            }
        }
    }
}