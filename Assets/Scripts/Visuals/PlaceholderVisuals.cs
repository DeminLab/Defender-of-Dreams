using UnityEngine;

namespace DefenderOfDreams.Visuals
{
    public static class PlaceholderVisuals
    {
        public static Sprite CreateBlockSprite(Color color, int sizePx = 16, int pixelsPerUnit = 16)
        {
            var tex = new Texture2D(sizePx, sizePx, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };
            var pixels = new Color[sizePx * sizePx];
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = color;
            tex.SetPixels(pixels);
            tex.Apply(false);
            return Sprite.Create(tex, new Rect(0, 0, sizePx, sizePx), new Vector2(0.5f, 0.5f), pixelsPerUnit);
        }

        public static GameObject CreateBlock(string name, Vector3 position, Color color, int sizePx = 16)
        {
            var go = new GameObject(name);
            go.transform.position = position;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = CreateBlockSprite(color, sizePx);
            return go;
        }

        public static void EnsurePlayerVisual(GameObject player)
        {
            if (player.GetComponent<SpriteRenderer>() != null)
                return;
            var sr = player.AddComponent<SpriteRenderer>();
            sr.sprite = CreateBlockSprite(new Color(0.75f, 0.85f, 1f, 1f), 16);
            sr.sortingOrder = 10;
        }
    }
}
