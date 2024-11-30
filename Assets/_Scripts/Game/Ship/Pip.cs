using CosmicShore.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class Pip : MonoBehaviour
{
    [SerializeField] Camera pipCamera;
    [SerializeField] bool mirrored;
    [SerializeField] IShip ship;

    // Start is called before the first frame update
    void Start()
    {
        if (ship.Player.GameCanvas != null) ship.Player.GameCanvas.MiniGameHUD.SetPipActive(!ship.AIPilot.AutoPilotEnabled, mirrored);
        if (pipCamera != null) pipCamera.gameObject.SetActive(!ship.AIPilot.AutoPilotEnabled);
    }
}
