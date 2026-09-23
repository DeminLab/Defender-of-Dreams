using UnityEngine;

namespace DefenderOfDreams.FogOfWar
{
    public class MemoryAnchor : MonoBehaviour
    {
        [SerializeField] private float radius = 8f;
        [SerializeField] private bool placedOnStart;

        private bool _active;

        private void Start()
        {
            if (placedOnStart)
                Activate();
        }

        public void Activate()
        {
            var fog = FogOfWarSystem.Instance;
            if (fog == null)
                return;
            if (!fog.TryPlaceAnchor(transform.position))
                return;
            _active = true;
        }

        public void DamageAnchor() => DestroyByEnemy();

        public void DestroyByEnemy()
        {
            if (!_active)
                return;
            var fog = FogOfWarSystem.Instance;
            if (fog != null)
                fog.DestroyAnchorsIn(transform.position, radius);
            _active = false;
            Destroy(gameObject);
        }
    }
}
