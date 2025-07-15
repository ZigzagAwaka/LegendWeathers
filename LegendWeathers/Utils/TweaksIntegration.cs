using System.Linq;
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
            var heatwave = new WeatherNameResolvable("heatwave");
            var solarflare = new WeatherNameResolvable("solarflare");
            var snowfall = new WeatherNameResolvable("snowfall");
            var blizzard = new WeatherNameResolvable("blizzard");
            //var toxicsmog = new WeatherNameResolvable("toxicsmog");
            var tornado = new WeatherNameResolvable("tornado");
            //var meteorshower = new WeatherNameResolvable("meteorshower");
            var blackout = new WeatherNameResolvable("blackout");
            var majoramoon = new WeatherNameResolvable("majoramoon");

            if (Plugin.config.majoraWeather.Value)
            {
                RegisterCombinedWeather("Rainy + Majora Moon", rainy, majoramoon);
                RegisterCombinedWeather("Stormy + Majora Moon", stormy, majoramoon);
                RegisterCombinedWeather("Eclipsed + Majora Moon", eclipsed, majoramoon);
                RegisterCombinedWeather("Majora Chaos", rainy, stormy, eclipsed, majoramoon);

                if (Plugin.config.LethalElementsInstalled)
                {
                    RegisterCombinedWeather("Heatwave + Majora Moon", heatwave, majoramoon);
                    RegisterCombinedWeather("Snowfall + Majora Moon", snowfall, majoramoon);
                    RegisterCombinedWeather("Blizzard + Majora Moon", blizzard, majoramoon);
                    RegisterCombinedWeather("Majora Climate Anomaly", heatwave, solarflare, snowfall, majoramoon);

                    string eowName = "The End of the World";
                    if (Plugin.config.CodeRebirthInstalled && Plugin.config.MrovWeathersInstalled)
                        RegisterCombinedWeather(eowName, rainy, stormy, eclipsed, heatwave, solarflare, snowfall, tornado, blackout, majoramoon);
                    else if (Plugin.config.CodeRebirthInstalled && !Plugin.config.MrovWeathersInstalled)
                        RegisterCombinedWeather(eowName, rainy, stormy, eclipsed, heatwave, solarflare, snowfall, tornado, majoramoon);
                    else if (!Plugin.config.CodeRebirthInstalled && Plugin.config.MrovWeathersInstalled)
                        RegisterCombinedWeather(eowName, rainy, stormy, eclipsed, heatwave, solarflare, snowfall, blackout, majoramoon);
                    else
                        RegisterCombinedWeather(eowName, rainy, stormy, eclipsed, heatwave, solarflare, snowfall, majoramoon);
                }

                if (Plugin.config.CodeRebirthInstalled)
                {
                    RegisterCombinedWeather("Tornado + Majora Moon", tornado, majoramoon);
                    RegisterCombinedWeather("Majora Superstorm", rainy, stormy, tornado, majoramoon);
                }

                if (Plugin.config.MrovWeathersInstalled)
                {
                    RegisterCombinedWeather("Blackout + Majora Moon", blackout, majoramoon);
                }
            }
        }

        private static void RegisterCombinedWeather(string name, params WeatherResolvable[] weathers)
        {
            new CombinedWeatherType(name, weathers.ToList());
        }
    }
}
