using UnityEngine;

namespace InfinitySword.Scripts
{
    public class Projectile : MonoBehaviour
    {
        public float damage;
        public float lifeTime;

        private void Start()
        {
            Destroy(gameObject, lifeTime);
        }
    }
}
