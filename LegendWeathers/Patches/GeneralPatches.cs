using HarmonyLib;
using LegendWeathers.Utils;
using LegendWeathers.Weathers;

namespace LegendWeathers.Patches
{
    [HarmonyPatch(typeof(StartOfRound))]
    internal class WeatherAlertPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("OnShipLandedMiscEvents")]
        public static void OnShipLandedMiscEventsPatch()
        {
            string title = "Weather alert!";
            if (Plugin.config.majoraWeather.Value && MajoraMoonWeather.MajoraMoonEffectReference != null && MajoraMoonWeather.MajoraMoonEffectReference.EffectEnabled)
            {
                Alert(title, MajoraMoonWeather.weatherAlert, true, "LW_MajoraTip");
            }
            if (Plugin.config.bloodMoonWeather.Value && BloodMoonWeather.BloodMoonEffectReference != null && BloodMoonWeather.BloodMoonEffectReference.EffectEnabled)
            {
                Alert(title, BloodMoonWeather.weatherAlert, true, "LW_BloodTip");
            }
        }

        private static void Alert(string title, string bottom, bool warning, string saveKey)
        {
            if (Plugin.config.generalWeatherAlertsSaved.Value)
                Effects.MessageOneTime(title, bottom, warning, saveKey);
            else
                Effects.Message(title, bottom, warning);
        }
    }
}
