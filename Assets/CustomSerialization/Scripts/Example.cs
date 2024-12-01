using System.Collections.Generic;
using UnityEngine;

namespace CustomSerialization
{
    public interface IDamageable
    {
    
    }

    public class Example : MonoBehaviour
    {
        public InterfaceReference<IDamageable> damageable;

        public InterfaceReference<IDamageable>[] referenceArray;

        [RequireInterface(typeof(IDamageable))]
        public MonoBehaviour attributeRestrictedToMB;

        [RequireInterface(typeof(IDamageable))]
        public ScriptableObject attributeRestrictedToSO;

        [RequireInterface(typeof(IDamageable))]
        public MonoBehaviour[] referenceWithAttributeArray;

        [RequireInterface(typeof(IDamageable))]
        public List<Object> referenceWithAttributeList;

        public List<InterfaceReference<IDamageable>> referenceList;

        public InterfaceReference<IDamageable, ScriptableObject> referenceRestrictedToSO;
        public InterfaceReference<IDamageable, MonoBehaviour> referenceRestrictedToMB;
    }
}
