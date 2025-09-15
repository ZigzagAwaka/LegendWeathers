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


    [HarmonyPatch(typeof(EnemyAI))]
    internal class BloodMoonEnemyResurrectionPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("KillEnemy")]
        public static void KillEnemyPatch(EnemyAI __instance, bool destroy)
        {
            if (!Plugin.config.bloodMoonWeather.Value || BloodMoonWeather.BloodMoonEffectReference == null || !BloodMoonWeather.BloodMoonEffectReference.EffectEnabled
                || __instance == null || __instance.enemyType == null || !__instance.IsServer)
            {
                return;
            }
            if ((!destroy || !__instance.enemyType.canBeDestroyed) && __instance.enemyType.canDie)
            {
                BloodMoonWeather.BloodMoonEffectReference.WorldObject?.GetComponent<BloodMoonWeather>()?.GetBloodMoonManager()?.ResurrectEnemy(__instance);
            }
        }
    }
}
