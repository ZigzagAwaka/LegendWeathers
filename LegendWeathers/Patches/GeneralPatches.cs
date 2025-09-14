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
            if (Plugin.config.majoraWeather.Value && MajoraMoonWeather.MajoraMoonEffectReference != null && MajoraMoonWeather.MajoraMoonEffectReference.EffectActive)
            {
                Effects.MessageOneTime(title, MajoraMoonWeather.weatherAlert, true, "LW_MajoraTip");
            }
        }
    }
}
