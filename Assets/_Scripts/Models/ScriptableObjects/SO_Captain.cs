using UnityEngine;

[CreateAssetMenu(fileName = "Spike Spiegel", menuName = "CosmicShore/Captain/Captain", order = 3)]
[System.Serializable]
public class SO_Captain : ScriptableObject
{
    [SerializeField] public string Name;
    [SerializeField] public string Description;
    [SerializeField] public string Flavor;
    [SerializeField] public Sprite Image;
    [SerializeField] public Sprite Icon;
    [SerializeField] public Sprite SelectedIcon;
    [SerializeField] public SO_Ship Ship;
    [SerializeField] public Element PrimaryElement;
    [SerializeField] public float InitialMass;
    [SerializeField] public float InitialCharge;
    [SerializeField] public float InitialSpace;
    [SerializeField] public float InitialTime;
}