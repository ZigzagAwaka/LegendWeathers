using System.Collections.Generic;
using WeatherRegistry;
using WeatherTweaks.Definitions;

namespace LegendWeathers.Utils
{
    internal class TweaksIntegration
    {
        public static void Setup()
        {
            var rainy = new WeatherTypeResolvable(LevelWeatherType.Rainy);
            var stormy = new WeatherTypeResolvable(LevelWeatherType.Stormy);
            //var foggy = new WeatherTypeResolvable(LevelWeatherType.Foggy);
            //var flooded = new WeatherTypeResolvable(LevelWeatherType.Flooded);
            var eclipsed = new WeatherTypeResolvable(LevelWeatherType.Eclipsed);

            if (Plugin.config.majoraWeather.Value)
            {
                var majoramoon = new WeatherNameResolvable("majoramoon");

                new CombinedWeatherType(
                    "Rainy + Majora Moon",
                    new List<WeatherResolvable>() { rainy, majoramoon }
                );
                new CombinedWeatherType(
                    "Stormy + Majora Moon",
                    new List<WeatherResolvable>() { stormy, majoramoon }
                );
                new CombinedWeatherType(
                    "Eclipsed + Majora Moon",
                    new List<WeatherResolvable>() { eclipsed, majoramoon }
                );
                new CombinedWeatherType(
                    "Majora Chaos",
                    new List<WeatherResolvable>() { rainy, stormy, eclipsed, majoramoon }
                );

                if (Plugin.config.LethalElementsInstalled)
                {
                    var heatwave = new WeatherNameResolvable("heatwave");
                    //var solarflare = new WeatherNameResolvable("solarflare");
                    var snowfall = new WeatherNameResolvable("snowfall");
                    var blizzard = new WeatherNameResolvable("blizzard");
                    //var toxicsmog = new WeatherNameResolvable("toxicsmog");

                    new CombinedWeatherType(
                        "Heatwave + Majora Moon",
                        new List<WeatherResolvable>() { heatwave, majoramoon }
                    );
                    new CombinedWeatherType(
                        "Snowfall + Majora Moon",
                        new List<WeatherResolvable>() { snowfall, majoramoon }
                    );
                    new CombinedWeatherType(
                        "Blizzard + Majora Moon",
                        new List<WeatherResolvable>() { blizzard, majoramoon }
                    );
                    new CombinedWeatherType(
                        "Majora Climate Anomaly",
                        new List<WeatherResolvable>() { heatwave, snowfall, majoramoon }
                    );
                    new CombinedWeatherType(
                        "The End of the World",
                        new List<WeatherResolvable>() { rainy, stormy, eclipsed, heatwave, snowfall, majoramoon }
                    );
                }
            }
        }
    }
}
