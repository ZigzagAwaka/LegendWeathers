using System.Collections.Generic;
using UnityEngine;

namespace LegendWeathers.WeatherSkyEffects
{
    public class BloodSkyEffect : SkyEffect
    {
        public GameObject? spawnedBloodSun = null;

        private readonly float bloodSunSizeFactor = 5.5f;

        private readonly List<Light> originalSunlights = new List<Light>();
        private readonly List<Color> originalSunlightColors = new List<Color>();
        private readonly Color bloodSunlightColor = new Color(1, 0.58f, 1f);

        public BloodSkyEffect() : base(Plugin.instance.bloodSkyObject)
        {
            overrideFogVolume = true;
        }

        public override void OnEnable()
        {
            if (WeatherRegistry.WeatherManager.IsSetupFinished)
            {
                base.OnEnable();
                var sunAnim = FindObjectOfType<animatedSun>();
                if (sunAnim != null)
                {
                    foreach (var light in sunAnim.gameObject.GetComponentsInChildren<Light>())
                    {
                        originalSunlights.Add(light);
                        originalSunlightColors.Add(light.color);
                        light.color = bloodSunlightColor;
                    }
                    var sunTextureTransform = sunAnim.transform.Find("SunTexture");
                    if (sunTextureTransform != null)
                    {
                        spawnedBloodSun = Instantiate(Plugin.instance.bloodSunObject, sunTextureTransform.position, sunTextureTransform.rotation, sunTextureTransform.parent);
                        if (spawnedBloodSun != null)
                        {
                            spawnedBloodSun.transform.localScale = sunTextureTransform.localScale * bloodSunSizeFactor;
                        }
                        else
                            Plugin.logger.LogError("Failed to instantiate SunBloodTexture.");
                    }
                    else
                        Plugin.logger.LogError("Failed to find SunTexture transform in the actual moon scene.");
                }
                else
                    Plugin.logger.LogError("Failed to find AnimatedSun component in the actual moon scene.");
            }
        }

        public override void OnDisable()
        {
            if (WeatherRegistry.WeatherManager.IsSetupFinished)
            {
                base.OnDisable();
                if (spawnedBloodSun != null)
                    Destroy(spawnedBloodSun);
                for (var i = 0; i < originalSunlights.Count; i++)
                {
                    originalSunlights[i].color = originalSunlightColors[i];
                }
                originalSunlights.Clear();
                originalSunlightColors.Clear();
            }
        }

        public override void Update()
        {

        }
    }
}