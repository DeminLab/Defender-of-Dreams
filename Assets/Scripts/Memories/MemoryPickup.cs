using DefenderOfDreams.Core;
using UnityEngine;

namespace DefenderOfDreams.Memories
{
    public class MemoryPickup : MonoBehaviour
    {
        [SerializeField] private int memoryId;
        [SerializeField] private string title = "Воспоминание";
        [TextArea]
        [SerializeField] private string text = "";
        [SerializeField] private bool destroyOnCollect = true;

        private bool _collected;

        private void Reset()
        {
            var col = GetComponent<Collider2D>();
            if (col != null)
                col.isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_collected || !other.CompareTag("Player"))
                return;
            Collect();
        }

        public void Collect()
        {
            if (_collected)
                return;
            if (MemoryLog.Collect(memoryId, title, text))
            {
                _collected = true;
                if (destroyOnCollect)
                    Destroy(gameObject);
                else
                    gameObject.SetActive(false);
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(0.6f, 0.85f, 1f, 0.9f);
            Gizmos.DrawWireSphere(transform.position, 0.35f);
        }
    }
}
