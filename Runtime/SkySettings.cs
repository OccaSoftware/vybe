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
        [ColorUsage(false, true)]
        public Color horizonDay = new Color(0.76f, 0.77f, 0.82f);

        [ColorUsage(false, true)]
        public Color skyDay = new Color(0.01f, 0.27f, 0.89f);

        [ColorUsage(false, true)]
        public Color horizonNight = new Color(0.52f, 0.35f, 0.53f);

        [ColorUsage(false, true)]
        public Color skyNight = new Color(0.08f, 0.09f, 0.32f);

        [Header("Sun")]
        [Range(0.9f, 1.0f)]
        public float sunSize = 0.999f;

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
            float starBrightness = Mathf.Lerp(0f, 10f, s);

            // Set
            skyMaterial.SetFloat(Params._SUN_SIZE, sunSize);
            skyMaterial.SetColor(Params._HORIZON_COLOR, horizon);
            skyMaterial.SetColor(Params._SKY_COLOR, sky);
            skyMaterial.SetFloat(Params._STAR_BRIGHTNESS, starBrightness);
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
