using HarmonyLib;
using LegendWeathers.Weathers;
using LegendWeathers.WeatherSkyEffects;

namespace LegendWeathers.Patches
{
    [HarmonyPatch(typeof(TimeOfDay))]
    internal class BloodMoonTimePatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("MoveTimeOfDay")]
        public static void MoveTimeOfDayPatch(TimeOfDay __instance)
        {
            if (Plugin.config.bloodMoonWeather.Value && __instance.sunAnimator != null &&
                BloodMoonWeather.BloodMoonEffectReference != null && BloodMoonWeather.BloodMoonEffectReference.EffectActive)
            {
                __instance.sunAnimator.SetFloat("timeOfDay", BloodSkyEffect.bloodMoonSunAnimatorFixedTime);
            }
        }
    }
}
