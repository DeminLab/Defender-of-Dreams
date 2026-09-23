using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DefenderOfDreams.Visuals
{
    public static class PrototypeVisualPolish
    {
        private static bool _done;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Apply()
        {
            if (_done || !Application.isPlaying)
                return;
            _done = true;

            LayoutHud();
            TintUnits();
            PolishGround();
            PolishCamera();
            AddUnitMotion();
            AddLabelOverhead();
        }

        private static void LayoutHud()
        {
            var canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null)
                return;

            var scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1280f, 720f);
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                scaler.matchWidthOrHeight = 0.5f;
            }

            var health = FindChild(canvas.transform, "HealthBar");
            if (health != null)
            {
                SetRect(health, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(24f, -24f), new Vector2(220f, 18f), new Vector2(0f, 1f));
                var img = health.GetComponent<Image>();
                if (img != null)
                {
                    img.color = new Color(0.85f, 0.15f, 0.15f, 1f);
                    img.type = Image.Type.Filled;
                    img.fillMethod = Image.FillMethod.Horizontal;
                    img.fillAmount = 1f;
                }
            }

            var anchors = FindChild(canvas.transform, "AnchorsText");
            if (anchors != null)
            {
                SetRect(anchors, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-24f, -24f), new Vector2(120f, 28f), new Vector2(1f, 1f));
                var t = anchors.GetComponent<Text>();
                if (t != null)
                {
                    t.alignment = TextAnchor.MiddleRight;
                    t.fontSize = 18;
                    t.fontStyle = FontStyle.Bold;
                    t.color = new Color(0.85f, 0.9f, 1f, 1f);
                    t.text = "0/3";
                }
            }

            var ability = FindChild(canvas.transform, "AbilityText");
            if (ability != null)
            {
                SetRect(ability, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-24f, -54f), new Vector2(120f, 24f), new Vector2(1f, 1f));
                var t = ability.GetComponent<Text>();
                if (t != null)
                {
                    t.alignment = TextAnchor.MiddleRight;
                    t.fontSize = 16;
                    t.color = new Color(0.75f, 1f, 0.85f, 1f);
                    t.text = "0/100";
                }
            }

            var forget = FindChild(canvas.transform, "ForgetTimerFill");
            if (forget != null)
            {
                SetRect(forget, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -28f), new Vector2(260f, 8f), new Vector2(0.5f, 1f));
                var img = forget.GetComponent<Image>();
                if (img != null)
                {
                    img.color = new Color(0.55f, 0.75f, 1f, 0.9f);
                    img.type = Image.Type.Filled;
                    img.fillMethod = Image.FillMethod.Horizontal;
                }
            }

            var dialog = FindChild(canvas.transform, "DialogPanel");
            if (dialog != null)
            {
                SetRect(dialog, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 48f), new Vector2(720f, 84f), new Vector2(0.5f, 0f));
                var img = dialog.GetComponent<Image>();
                if (img != null)
                    img.color = new Color(0f, 0f, 0f, 0.72f);
            }

            var dialogTextGo = FindChild(canvas.transform, "DialogText") ?? FindChild(dialog, "DialogText");
            if (dialogTextGo != null)
            {
                SetRect(dialogTextGo, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Vector2(0.5f, 0.5f));
                var t = dialogTextGo.GetComponent<Text>();
                if (t != null)
                {
                    t.alignment = TextAnchor.MiddleCenter;
                    t.fontSize = 20;
                    t.color = Color.white;
                    var rt = t.rectTransform;
                    rt.offsetMin = new Vector2(24f, 12f);
                    rt.offsetMax = new Vector2(-24f, -12f);
                }
            }

            var pause = FindChild(canvas.transform, "PausePanel");
            if (pause != null)
            {
                SetRect(pause, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Vector2(0.5f, 0.5f));
                var img = pause.GetComponent<Image>();
                if (img != null)
                    img.color = new Color(0f, 0f, 0f, 0.8f);
            }
        }

        private static void TintUnits()
        {
            foreach (var mr in Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None))
            {
                var go = mr.gameObject;
                var name = go.name;

                if (name.Contains("Player"))
                    ApplyColor(mr, new Color(0.35f, 0.75f, 1f, 1f), 0.25f);
                else if (name.Contains("Shoroh"))
                    ApplyColor(mr, new Color(1f, 0.35f, 0.3f, 1f), 0.35f);
                else if (name.Contains("Poziratel"))
                    ApplyColor(mr, new Color(1f, 0.65f, 0.2f, 1f), 0.3f);
                else if (name.Contains("Ground"))
                    ApplyColor(mr, new Color(0.22f, 0.34f, 0.28f, 1f), 0f);
                else if (name.Contains("FogOverlay"))
                    continue;
            }

            foreach (var sr in Object.FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None))
            {
                if (sr.gameObject.name.Contains("Player"))
                    sr.color = new Color(0.4f, 0.8f, 1f, 1f);
            }
        }

        private static void ApplyColor(MeshRenderer mr, Color color, float emission)
        {
            var materials = new List<Material>(mr.sharedMaterials);
            for (int i = 0; i < materials.Count; i++)
            {
                var src = materials[i];
                if (src == null)
                    continue;
                var mat = Object.Instantiate(src);
                mat.name = src.name + " (Polished)";
                if (mat.HasProperty("_BaseColor"))
                    mat.SetColor("_BaseColor", color);
                if (mat.HasProperty("_Color"))
                    mat.SetColor("_Color", color);
                if (emission > 0f && mat.HasProperty("_EmissionColor"))
                {
                    mat.EnableKeyword("_EMISSION");
                    mat.SetColor("_EmissionColor", color * emission);
                }
                materials[i] = mat;
            }
            mr.sharedMaterials = materials.ToArray();
        }

        private static void PolishGround()
        {
            var ground = GameObject.Find("Ground");
            if (ground == null)
                return;
            var t = ground.transform;
            if (t.localScale.x < 10f)
                t.localScale = new Vector3(20f, 1f, 20f);
        }

        private static void PolishCamera()
        {
            var cam = Camera.main;
            if (cam == null)
                return;
            cam.backgroundColor = new Color(0.07f, 0.09f, 0.14f, 1f);
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.orthographic = true;
            if (cam.orthographicSize < 6f)
                cam.orthographicSize = 6.5f;
        }

        private static void AddUnitMotion()
        {
            foreach (var go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
            {
                var n = go.name;
                bool isUnit = n.Contains("Player") || n.Contains("Shoroh") || n.Contains("Poziratel");
                if (!isUnit)
                    continue;
                if (go.GetComponent<UnitBob>() != null)
                    continue;
                var bob = go.AddComponent<UnitBob>();
                bob.isPlayer = n.Contains("Player");
            }
        }

        private static void AddLabelOverhead()
        {
            foreach (var go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
            {
                var n = go.name;
                string label = null;
                Color color = Color.white;
                if (n.Contains("Player")) { label = "НЕРО"; color = new Color(0.55f, 0.85f, 1f); }
                else if (n.Contains("Shoroh")) { label = "ШОРОХ"; color = new Color(1f, 0.45f, 0.4f); }
                else if (n.Contains("Poziratel")) { label = "ПОЗИРАТЕЛЬ"; color = new Color(1f, 0.7f, 0.3f); }
                if (label == null)
                    continue;
                if (go.GetComponentInChildren<TextMesh>() != null)
                    continue;

                var tmGo = new GameObject("Label");
                tmGo.transform.SetParent(go.transform, false);
                tmGo.transform.localPosition = new Vector3(0f, 1.4f, 0f);
                tmGo.transform.localScale = Vector3.one * 0.12f;
                var tm = tmGo.AddComponent<TextMesh>();
                tm.text = label;
                tm.fontSize = 64;
                tm.characterSize = 1f;
                tm.anchor = TextAnchor.LowerCenter;
                tm.alignment = TextAlignment.Center;
                tm.color = color;
                var mesh = tmGo.GetComponent<MeshRenderer>();
                if (mesh != null)
                    mesh.sortingOrder = 50;
            }
        }

        private static Transform FindChild(Transform root, string name)
        {
            if (root == null)
                return null;
            if (root.name == name)
                return root;
            for (int i = 0; i < root.childCount; i++)
            {
                var hit = FindChild(root.GetChild(i), name);
                if (hit != null)
                    return hit;
            }
            return null;
        }

        private static void SetRect(Transform t, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 size, Vector2 pivot)
        {
            if (t is not RectTransform rt)
                return;
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;
            rt.localScale = Vector3.one;
            rt.localRotation = Quaternion.identity;
        }
    }
}
