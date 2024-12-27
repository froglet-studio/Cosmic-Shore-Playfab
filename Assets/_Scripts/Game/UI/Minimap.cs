using CosmicShore.Core;
using UnityEngine;

namespace CosmicShore.Game.UI
{
    public class Minimap : MonoBehaviour
    {
        [SerializeField] Camera Camera;
        [SerializeField] IPlayer Player;        // TODO - Make sure this is initialized
        [SerializeField] float CameraRadius;
        [SerializeField] Node activeNode;

        IShip ship;

        void Start()
        {
            ship = Player.Ship;
        }

        void Update()
        {
            Camera.transform.position = (-ship.Transform.forward * CameraRadius) + activeNode.transform.position;
            Camera.transform.LookAt(activeNode.transform.position, ship.Transform.up);
        }

        public void SetActiveNode(Node node)
        {
            activeNode = node;
        }
    }
}