using DefenderOfDreams.Core;
using DefenderOfDreams.FogOfWar;
using UnityEngine;

namespace DefenderOfDreams.Gameplay
{
    public class ZoneForgetTrigger : MonoBehaviour
    {
        [SerializeField] private string zoneId = "zone_1";
        [SerializeField] private float pressurePerSecond = 100f / 30f;
        [SerializeField] private int damageOnFail = 1;

        private float _pressure;
        private bool _active;
        private bool _failed;

        public string ZoneId => zoneId;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player") || _failed)
                return;
            _active = true;
            _pressure = 0f;
            var fog = FogOfWarSystem.Instance;
            if (fog != null)
                fog.MarkZoneActive(zoneId);
            GameEvents.RaiseActiveZoneForgettingChanged(zoneId, 0f, 100f);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!other.CompareTag("Player"))
                return;
            _active = false;
            GameEvents.RaiseActiveZoneForgettingChanged(zoneId, -1f, 100f);
        }

        private void Update()
        {
            if (!_active || _failed)
                return;

            _pressure += pressurePerSecond * Time.deltaTime;
            float norm = Mathf.Clamp01(_pressure / 100f);

            var fog = FogOfWarSystem.Instance;
            if (fog != null)
                fog.SetActiveZonePressure(zoneId, norm);

            GameEvents.RaiseActiveZoneForgettingChanged(zoneId, _pressure, 100f);

            if (_pressure >= 100f)
                Fail();
        }

        public void Complete()
        {
            if (_failed)
                return;
            _active = false;
            _pressure = 0f;
            GameFlags.Set("zone_complete_" + zoneId, true);
            var fog = FogOfWarSystem.Instance;
            if (fog != null)
                fog.SetActiveZonePressure(zoneId, 0f);
            GameEvents.RaiseActiveZoneForgettingChanged(zoneId, -1f, 100f);
            enabled = false;
        }

        private void Fail()
        {
            _failed = true;
            _active = false;
            _pressure = 0f;

            var fog = FogOfWarSystem.Instance;
            if (fog != null)
            {
                fog.ForgetZone(zoneId);
                foreach (var anchor in Object.FindObjectsByType<MemoryAnchor>())
                    anchor.DamageAnchor();
            }

            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                var health = player.GetComponent<Combat.Health>();
                health?.TakeDamage(damageOnFail, Vector2.zero);
            }

            GameFlags.Set("zone_failed_" + zoneId, true);
            GameEvents.RaiseActiveZoneForgettingChanged(zoneId, -1f, 100f);
        }
    }
}
