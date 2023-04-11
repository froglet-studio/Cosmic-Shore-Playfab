using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Ship List", menuName = "TailGlider/ShipList", order = 10)]
[System.Serializable]
public class SO_ShipList : ScriptableObject
{
    public List<SO_Ship> ShipList;
}