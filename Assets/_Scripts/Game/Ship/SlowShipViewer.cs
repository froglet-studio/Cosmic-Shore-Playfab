using CosmicShore.Core;
using CosmicShore.Game;
using UnityEngine;

namespace CosmicShore
{
    public class SlowShipViewer : MonoBehaviour
    {
        [SerializeField] Material trailViewerMaterial;

        LineRenderer _lineRenderer;
        IPlayer _player;
        IShip _ship;
        Transform _targetTransform;
        
        void Start()
        {
            _ship = GetComponent<IShip>();  
            _lineRenderer = gameObject.AddComponent<LineRenderer>();
            _lineRenderer.material = trailViewerMaterial;
            _lineRenderer.startWidth = _lineRenderer.endWidth = 0.1f;
        }

        public void Initialize(IPlayer player)
        {
            _player = player;
        }

        void Update()
        {
            _targetTransform = null;
            _lineRenderer.SetPosition(0, transform.position);
            _lineRenderer.enabled = false;
            if (Hangar.Instance.SlowedShipTransforms.Count > 0 && _player != null && _player.Ship == _ship)
            {
                var distance = float.PositiveInfinity;
                foreach (var shipTransform in Hangar.Instance.SlowedShipTransforms)
                {
                    if (shipTransform == transform) continue;
                    float tempDistance;     
                    tempDistance = (shipTransform.position - transform.position).magnitude;
                    if (tempDistance < distance)
                    {
                        distance = tempDistance;
                        _targetTransform = shipTransform;
                    }
                }
                if (_targetTransform != null)
                {
                    _lineRenderer.SetPosition(1, _targetTransform.position);
                    _lineRenderer.enabled = true;
                }
            }
        }
    }
}