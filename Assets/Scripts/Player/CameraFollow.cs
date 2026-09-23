using UnityEngine;

namespace DefenderOfDreams.Player
{
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private float smoothTime = 0.18f;
        [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);
        [SerializeField] private bool orthographic = true;
        [SerializeField] private float orthographicSize = 6f;

        private Vector3 _velocity;

        public void SetTarget(Transform t) => target = t;

        private void Start()
        {
            var cam = GetComponent<Camera>();
            if (cam != null)
            {
                cam.orthographic = orthographic;
                cam.orthographicSize = orthographicSize;
            }

            if (target != null)
                transform.position = target.position + offset;
        }

        private void LateUpdate()
        {
            if (target == null)
                return;
            Vector3 desired = target.position + offset;
            desired.z = offset.z;
            transform.position = Vector3.SmoothDamp(transform.position, desired, ref _velocity, smoothTime);
        }
    }
}
