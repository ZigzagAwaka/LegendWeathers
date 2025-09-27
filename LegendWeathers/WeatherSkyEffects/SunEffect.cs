using LegendWeathers.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace LegendWeathers.WeatherSkyEffects
{
    public class SunEffect : SkyEffect
    {
        public GameObject? spawnedSun = null;

        public float sunSizeFactor = 1f;
        public bool modifySunlightColor = false;
        public Color sunlightColor = new Color(1, 1, 1);

        private readonly List<Light> originalSunlights = new List<Light>();
        private readonly List<Color> originalSunlightColors = new List<Color>();

        public SunEffect(GameObject? gameObject) : base(gameObject) { }

        public override void OnEnable()
        {
            if (!WeatherRegistry.WeatherManager.IsSetupFinished)
                return;
            SetupCustomSun();
            if (!IsEffectActive)
                Plugin.logger.LogWarning("Failed to setup " + effectName + " Custom Sun Effect. The effect could be rendered incorrectly.");
            if (overrideFogVolume)
                Effects.EnableVanillaVolumeFog(false, ref fogVolumeComponentExists);
        }

        public override void OnDisable()
        {
            if (!WeatherRegistry.WeatherManager.IsSetupFinished)
                return;
            if (spawnedSun != null)
            {
                IsEffectActive = false;
                Destroy(spawnedSun);
                spawnedSun = null;
                if (modifySunlightColor)
                {
                    for (var i = 0; i < originalSunlights.Count; i++)
                    {
                        if (originalSunlights[i] != null)
                            originalSunlights[i].color = originalSunlightColors[i];
                    }
                    originalSunlights.Clear();
                    originalSunlightColors.Clear();
                }
            }
            if (overrideFogVolume)
                Effects.EnableVanillaVolumeFog(true, ref fogVolumeComponentExists);
        }

        public override void Update() { }

        private void SetupCustomSun()
        {
            var sunAnim = FindObjectOfType<animatedSun>();
            if (sunAnim != null)
            {
                if (modifySunlightColor)
                {
                    foreach (var light in sunAnim.gameObject.GetComponentsInChildren<Light>())
                    {
                        originalSunlights.Add(light);
                        originalSunlightColors.Add(light.color);
                        light.color = sunlightColor;
                    }
                }
                var sunTextureTransform = sunAnim.transform.Find("SunTexture");
                if (sunTextureTransform != null)
                {
                    spawnedSun = Instantiate(effectGameObject, sunTextureTransform.position, sunTextureTransform.rotation, sunTextureTransform.parent);
                    if (spawnedSun != null)
                    {
                        spawnedSun.transform.localScale = sunTextureTransform.localScale * sunSizeFactor;
                        IsEffectActive = true;
                    }
                    else
                        Plugin.logger.LogError("Failed to instantiate custom sun texture.");
                }
                else
                    Plugin.logger.LogError("Failed to find SunTexture transform in the actual moon scene.");
            }
            else
            {
                Plugin.logger.LogError("Failed to find AnimatedSun component in the actual moon scene.");
            }
        }
    }
}
