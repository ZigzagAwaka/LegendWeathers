using UnityEngine;
using WeatherRegistry.Editor;

namespace LegendWeathers.Weathers
{
    public class LegendWeather : MonoBehaviour
    {
        public readonly WeatherDefinition weatherDefinition;

        public LegendWeather(WeatherDefinition? definition)
        {
            if (definition == null)
            {
                throw new System.NullReferenceException("WeatherDefinition is null.");
            }
            weatherDefinition = definition;
        }

        public virtual void OnEnable()
        {
            if (WeatherRegistry.WeatherManager.IsSetupFinished)
                Plugin.logger.LogInfo(weatherDefinition.Name + " Weather is enabled !");
        }

        public virtual void OnDisable()
        {
            if (WeatherRegistry.WeatherManager.IsSetupFinished)
                Plugin.logger.LogInfo(weatherDefinition.Name + " Weather is destroyed.");
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
    }
}
