using System.Collections;
using System.ComponentModel;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

namespace StarWriter.Core
{
    public class LoadedGun : Gun
    {
        [Header("Projectile Configuration")]
        [SerializeField] float speed = 20;
        [SerializeField] float projectileTime = 3;
        [SerializeField] FiringPatterns firingPattern = FiringPatterns.DoubleHexRing;

        public void FireGun()
        {
            GameObject Container = new GameObject();
            Container.transform.parent = transform;
            FireGun(Container.transform, speed, Vector3.zero, 1, true, 0, projectileTime, firingPattern); // charge could be used to limit recursion depth
        }

        //public void FireGun(Transform containerTransform, float speed, Vector3 inheritedVelocity,
        //    float projectileScale, bool ignoreCooldown = false, float projectileTime = 3, float charge = 0, FiringPatterns firingPattern = FiringPatterns.single)
    }
}