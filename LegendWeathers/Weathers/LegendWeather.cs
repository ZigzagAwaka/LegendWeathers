﻿using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace LegendWeathers.Weathers
{
    public class LegendWeather : MonoBehaviour
    {
        public class WeatherInfo
        {
            public string name;
            public int weight;
            public float scrapAmount;
            public float scrapValue;
            public Color color;

            public WeatherInfo(string name, int weight, float scrapAmount, float scrapValue, Color color)
            {
                this.name = name;
                this.weight = weight;
                this.scrapAmount = scrapAmount;
                this.scrapValue = scrapValue;
                this.color = color;
            }
        }

        private readonly WeatherInfo weatherInfo;
        private bool fogVolumeComponentExists = false;

        public LegendWeather(WeatherInfo info)
        {
            weatherInfo = info;
        }

        public virtual void OnEnable()
        {
            if (WeatherRegistry.WeatherManager.IsSetupFinished)
                Plugin.logger.LogInfo(weatherInfo.name + " Weather is enabled !");
        }

        public virtual void OnDisable()
        {
            if (WeatherRegistry.WeatherManager.IsSetupFinished)
                Plugin.logger.LogInfo(weatherInfo.name + " Weather is destroyed.");
        }

        public void EnableVanillaSun(bool enabled)
        {
            try
            {
                var sunAnim = FindObjectOfType<animatedSun>();
                if (sunAnim != null)
                {
                    var sunTextureTransform = sunAnim.transform.Find("SunTexture");
                    sunTextureTransform?.gameObject?.SetActive(enabled);
                    var eclipseObjectTransform = sunAnim.transform.Find("EclipseObject");
                    eclipseObjectTransform?.gameObject?.SetActive(enabled);
                }
            }
            catch (System.Exception)
            {
                Plugin.logger.LogInfo("Failed to " + (enabled ? "enable" : "disable") + " vanilla sun. Probably not an error if the ship is going back in orbit.");
            }
        }

        public void EnableVanillaVolumeFog(bool enabled)
        {
            try
            {
                if (enabled && !fogVolumeComponentExists)
                    return;
                foreach (var volume in FindObjectsOfType<Volume>())
                {
                    if (volume == null || volume.profile == null || volume.gameObject.scene.name != RoundManager.Instance.currentLevel.sceneName)
                        continue;
                    foreach (var component in volume.profile.components)
                    {
                        if (component.active && component is Fog)
                        {
                            component.active = enabled;
                            fogVolumeComponentExists = !enabled;
                        }
                    }
                }
            }
            catch (System.Exception)
            {
                Plugin.logger.LogWarning("Failed to " + (enabled ? "enable" : "disable") + " vanilla volume fog.");
            }
        }
    }
}
