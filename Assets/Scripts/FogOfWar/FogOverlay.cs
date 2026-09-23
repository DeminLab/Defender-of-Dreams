using UnityEngine;

namespace DefenderOfDreams.FogOfWar
{
    [RequireComponent(typeof(MeshRenderer))]
    public class FogOverlay : MonoBehaviour
    {
        private static readonly int FogTexId = Shader.PropertyToID("_FogTex");
        private static readonly int WorldOriginId = Shader.PropertyToID("_WorldOrigin");
        private static readonly int WorldSizeId = Shader.PropertyToID("_WorldSize");
        private static readonly int FogColorId = Shader.PropertyToID("_FogColor");
        private static readonly int AccentColorId = Shader.PropertyToID("_AccentColor");
        private static readonly int MaxOpacityId = Shader.PropertyToID("_MaxOpacity");
        private static readonly int DitherIntensityId = Shader.PropertyToID("_DitherIntensity");
        private static readonly int DitherSpeedId = Shader.PropertyToID("_DitherSpeed");
        private static readonly int SimplifiedId = Shader.PropertyToID("_Simplified");

        [SerializeField] private Camera targetCamera;
        [SerializeField] private float zOffset = -1f;
        [SerializeField] private Material materialOverride;

        private MeshFilter _filter;
        private MeshRenderer _renderer;
        private Material _material;
        private FogOfWarSystem _fog;

        private void Awake()
        {
            _filter = GetComponent<MeshFilter>();
            _renderer = GetComponent<MeshRenderer>();
            if (targetCamera == null)
                targetCamera = Camera.main;

            var shader = materialOverride != null ? materialOverride.shader : Shader.Find("DefenderOfDreams/FogOverlay");
            _material = materialOverride != null ? materialOverride : new Material(shader);
            _renderer.sharedMaterial = _material;
            _filter.sharedMesh = BuildQuad();
        }

        private void LateUpdate()
        {
            _fog = FogOfWarSystem.Instance;
            if (_fog == null || targetCamera == null)
                return;

            var settings = _fog.Settings;
            _material.SetTexture(FogTexId, _fog.MaskTexture);
            _material.SetVector(WorldOriginId, _fog.WorldOrigin);
            _material.SetVector(WorldSizeId, _fog.WorldSize);
            _material.SetColor(FogColorId, settings.fogColor);
            _material.SetColor(AccentColorId, settings.fogAccentColor);
            _material.SetFloat(MaxOpacityId, settings.fogMaxOpacity);
            bool simple = settings.simplifiedFallback || !settings.ditherEnabled;
            _material.SetFloat(DitherIntensityId, simple ? 0f : settings.ditherIntensity);
            _material.SetFloat(DitherSpeedId, settings.ditherSpeed);
            _material.SetFloat(SimplifiedId, simple ? 1f : 0f);

            transform.position = targetCamera.transform.position + Vector3.forward * zOffset;
        }

        private void OnDestroy()
        {
            if (_material != null && materialOverride == null)
                Destroy(_material);
        }

        private static Mesh BuildQuad()
        {
            var mesh = new Mesh
            {
                name = "FogOverlayQuad",
                vertices = new[]
                {
                    new Vector3(-1f, -1f, 0f),
                    new Vector3(1f, -1f, 0f),
                    new Vector3(-1f, 1f, 0f),
                    new Vector3(1f, 1f, 0f)
                },
                uv = new[]
                {
                    new Vector2(0f, 0f),
                    new Vector2(1f, 0f),
                    new Vector2(0f, 1f),
                    new Vector2(1f, 1f)
                },
                triangles = new[] { 0, 2, 1, 2, 3, 1 }
            };
            mesh.RecalculateBounds();
            return mesh;
        }
    }
}
