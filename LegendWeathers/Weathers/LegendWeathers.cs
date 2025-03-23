using UnityEngine;

namespace LegendWeathers.Weathers
{
    public class LegendWeathers : MonoBehaviour
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

        public LegendWeathers(WeatherInfo info)
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
    }
}
