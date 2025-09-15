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
                Effects.MessageOneTime(title, MajoraMoonWeather.weatherAlert, true, "LW_MajoraTip");
            }
            if (Plugin.config.bloodMoonWeather.Value && BloodMoonWeather.BloodMoonEffectReference != null && BloodMoonWeather.BloodMoonEffectReference.EffectEnabled)
            {
                Effects.MessageOneTime(title, BloodMoonWeather.weatherAlert, true, "LW_BloodTip");
            }
        }
    }
}
