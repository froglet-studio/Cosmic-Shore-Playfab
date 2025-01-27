using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Pilot List", menuName = "CosmicShore/PilotList", order = 11)]
[System.Serializable]
public class SO_PilotList : ScriptableObject
{
    public List<SO_Pilot> PilotList;
}