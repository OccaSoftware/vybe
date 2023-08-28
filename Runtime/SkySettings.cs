using System;

using UnityEngine;
using UnityEngine.Rendering;

namespace OccaSoftware.Vybe.Runtime
{
    [ExecuteAlways]
    [RequireComponent(typeof(Light))]
    [ReloadGroup]
    public class SkySettings : MonoBehaviour
    {
        [Header("Colors")]
        [SerializeField, ColorUsage(false, true)]
        private Color horizonDay = new Color(0.76f, 0.77f, 0.82f);

        [SerializeField, ColorUsage(false, true)]
        private Color skyDay = new Color(0.01f, 0.27f, 0.89f);

        [SerializeField, ColorUsage(false, true)]
        private Color horizonNight = new Color(0.52f, 0.35f, 0.53f);

        [SerializeField, ColorUsage(false, true)]
        private Color skyNight = new Color(0.08f, 0.09f, 0.32f);

        [Header("Sun")]
        [SerializeField, Range(0.0f, 1.0f)]
        private float sunSize = 0.05f;

        [Header("Stars")]
        [SerializeField, Min(0f)]
        private float starBrightness = 1.0f;

        private float shaderSunSize = 0.999f;

        private void OnValidate()
        {
            SetSunSize(sunSize);
        }

        public void SetSunSize(float size)
        {
            shaderSunSize = 1.0f - Mathf.Clamp01(size) * 0.1f;
        }

        [Header("Static Properties")]
        [SerializeField]
        [Reload("Runtime/StarTexture.asset")]
        private Texture3D starTexture;

        [SerializeField]
        [Reload("Runtime/Vybe.shader")]
        private Shader shader;

        private Material skyMaterial = null;
        private Light mainLight;

        void OnEnable()
        {
#if UNITY_EDITOR
            ResourceReloader.ReloadAllNullIn(this, "Packages/com.occasoftware.vybe");
#endif

            skyMaterial = CoreUtils.CreateEngineMaterial(shader);
            RenderSettings.skybox = skyMaterial;

            mainLight = GetComponent<Light>();
            RenderSettings.sun = mainLight;
        }

        private void OnDisable()
        {
            CoreUtils.Destroy(skyMaterial);
            skyMaterial = null;
        }

        void Update()
        {
            if (mainLight == null)
                return;

            // Calculate Factor
            float s = Vector3.Dot(Vector3.up, mainLight.transform.forward);
            s += 1.0f;
            s *= 0.5f;

            // Interpolate Colors
            Color horizon = Color.LerpUnclamped(horizonDay, horizonNight, s);
            Color sky = Color.LerpUnclamped(skyDay, skyNight, s);
            RenderSettings.ambientEquatorColor = horizon;
            RenderSettings.ambientGroundColor = Color.black;
            RenderSettings.ambientSkyColor = sky;
            float starBrightnessInShader = Mathf.Lerp(0f, 10f, s) * starBrightness;

            // Set
            skyMaterial.SetFloat(Params._SUN_SIZE, shaderSunSize);
            skyMaterial.SetColor(Params._HORIZON_COLOR, horizon);
            skyMaterial.SetColor(Params._SKY_COLOR, sky);
            skyMaterial.SetFloat(Params._STAR_BRIGHTNESS, starBrightnessInShader);
            skyMaterial.SetTexture(Params._STAR_TEXTURE, starTexture);
        }

        private static class Params
        {
            public static int _HORIZON_COLOR = Shader.PropertyToID("_HORIZON_COLOR");
            public static int _SKY_COLOR = Shader.PropertyToID("_SKY_COLOR");
            public static int _SUN_SIZE = Shader.PropertyToID("_SUN_SIZE");
            public static int _STAR_TEXTURE = Shader.PropertyToID("_STAR_TEXTURE");
            public static int _STAR_BRIGHTNESS = Shader.PropertyToID("_STAR_BRIGHTNESS");
        }
    }
}
