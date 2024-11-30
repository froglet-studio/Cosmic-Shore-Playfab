using CosmicShore.Game;
using System.Collections;
using UnityEngine;

public class PortraitUI : MonoBehaviour
{
    [SerializeField] Vector2 anchorMin;
    [SerializeField] Vector2 anchorMax;
    [SerializeField] Vector2 position;
    [SerializeField] Vector2 portraitAnchorMin;
    [SerializeField] Vector2 portraitAnchorMax;
    [SerializeField] Vector2 portraitPosition;

    RectTransform rectTransform;
    bool playerReady;

    IPlayer _player;

    // TODO - Need to call this method from MiniGame or somewhere
    // keeping in mind for both single player and multiplayer
    public void Initialize(IPlayer player)
    {
        _player = player;
        playerReady = true;
    }

    void Update()
    {
        if (!playerReady) return;

        if (_player.Ship.InputController.Portrait)
        {
            // Set the anchorMin and anchorMax values to center the RectTransform
            rectTransform.anchorMin = portraitAnchorMin;
            rectTransform.anchorMax = portraitAnchorMax;

            // Set the position of the RectTransform
            rectTransform.localPosition = portraitPosition;

            // Rotate the RectTransform 90 degrees around the z-axis
            rectTransform.rotation = Quaternion.Euler(0f, 0f, 90f);
        }
        else
        {
            // Set the anchorMin and anchorMax values to center the RectTransform
            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;

            // Set the position of the RectTransform
            rectTransform.localPosition = position;

            // Rotate the RectTransform 90 degrees around the z-axis
            rectTransform.rotation = Quaternion.Euler(0f, 0f, 0f);
        }
    }
}
