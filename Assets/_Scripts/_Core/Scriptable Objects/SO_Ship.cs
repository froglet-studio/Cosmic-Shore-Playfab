using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "New Ship", menuName = "CosmicShore/Ship", order = 1)]
[System.Serializable]
public class SO_Ship : ScriptableObject
{
    [SerializeField] public ShipTypes Class;

    [SerializeField] public string Name;
    [SerializeField] public string Description;
    [SerializeField] public Sprite Icon;
    [SerializeField] public Sprite SelectedIcon;
    [SerializeField] public Sprite PreviewImage;
    [SerializeField] public Sprite TrailPreviewImage;
    [SerializeField] public List<SO_ShipAbility> Abilities;
    [SerializeField] public List<SO_Pilot> Pilots;
    [FormerlySerializedAs("MiniGames")]
    [SerializeField] public List<SO_ArcadeGame> TrainingGames;
}