using System.Linq;
using UnityEngine;
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
            var meteorshower = new WeatherNameResolvable("meteorshower");
            var blackout = new WeatherNameResolvable("blackout");
            //var blackfog = new WeatherNameResolvable("blackfog");
            //var blue = new WeatherNameResolvable("blue");
            //var forsaken = new WeatherNameResolvable("forsaken");
            //var hallowed = new WeatherNameResolvable("hallowed");
            var majoramoon = new WeatherNameResolvable("majoramoon");
            //var bloodmoon = new WeatherNameResolvable("bloodmoon");

            if (Plugin.config.majoraWeather.Value)
            {
                RegisterCombinedWeather("Rainy + Majora Moon", rainy, majoramoon);
                RegisterCombinedWeather("Stormy + Majora Moon", stormy, majoramoon);
                RegisterCombinedWeather("Eclipsed + Majora Moon", eclipsed, majoramoon);
                RegisterCombinedWeather("Majora Chaos", Color.magenta, rainy, stormy, eclipsed, majoramoon);

                if (Compatibility.LethalElementsInstalled)
                {
                    RegisterCombinedWeather("Heatwave + Majora Moon", heatwave, majoramoon);
                    RegisterCombinedWeather("Snowfall + Majora Moon", snowfall, majoramoon);
                    RegisterCombinedWeather("Blizzard + Majora Moon", blizzard, majoramoon);
                    RegisterCombinedWeather("Majora Climate Anomaly", Color.yellow, heatwave, solarflare, snowfall, majoramoon);

                    string eowName = "The End of the World";
                    if (Compatibility.CodeRebirthInstalled && Compatibility.MrovWeathersInstalled)
                        RegisterCombinedWeather(eowName, Color.magenta, rainy, stormy, eclipsed, heatwave, solarflare, snowfall, tornado, meteorshower, blackout, majoramoon);
                    else if (Compatibility.CodeRebirthInstalled && !Compatibility.MrovWeathersInstalled)
                        RegisterCombinedWeather(eowName, Color.magenta, rainy, stormy, eclipsed, heatwave, solarflare, snowfall, tornado, meteorshower, majoramoon);
                    else if (!Compatibility.CodeRebirthInstalled && Compatibility.MrovWeathersInstalled)
                        RegisterCombinedWeather(eowName, Color.magenta, rainy, stormy, eclipsed, heatwave, solarflare, snowfall, blackout, majoramoon);
                    else
                        RegisterCombinedWeather(eowName, Color.magenta, rainy, stormy, eclipsed, heatwave, solarflare, snowfall, majoramoon);
                }

                if (Compatibility.CodeRebirthInstalled)
                {
                    RegisterCombinedWeather("Tornado + Majora Moon", tornado, majoramoon);
                    RegisterCombinedWeather("Majora Superstorm", Color.red, rainy, stormy, tornado, majoramoon);
                    RegisterCombinedWeather("Meteor Shower + Majora Moon", meteorshower, majoramoon);
                }

                if (Compatibility.MrovWeathersInstalled)
                {
                    RegisterCombinedWeather("Blackout + Majora Moon", blackout, majoramoon);
                }
            }
        }

        private static void RegisterCombinedWeather(string name, params WeatherResolvable[] weathers)
        {
            new CombinedWeatherType(name, weathers.ToList());
        }

        private static void RegisterCombinedWeather(string name, Color nameColor, params WeatherResolvable[] weathers)
        {
            new CombinedWeatherType(name, weathers.ToList())
            {
                Color = nameColor
            };
        }
    }
}
