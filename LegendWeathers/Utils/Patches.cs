using HarmonyLib;
using LegendWeathers.BehaviourScripts;
using LegendWeathers.Weathers;
using System.Linq;
using UnityEngine;

namespace LegendWeathers.Utils
{
    [HarmonyPatch(typeof(StartOfRound))]
    internal class WeatherAlertPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("OnShipLandedMiscEvents")]
        public static void OnShipLandedMiscEventsPatch()
        {
            string title = "Weather alert!";
            if (Plugin.config.majoraWeather.Value && Effects.IsWeatherEffectPresent("majoramoon"))
            {
                Effects.MessageOneTime(title, MajoraMoonWeather.weatherAlert, true, "LW_MajoraTip");
            }
        }
    }


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
                __instance.normalizedTimeOfDay = __instance.currentDayTime / __instance.totalTime;
                var originalTime = Mathf.Clamp(__instance.normalizedTimeOfDay, 0f, 0.99f);
                __instance.sunAnimator.SetFloat("timeOfDay", originalTime * 0.3f / 0.99f);
            }
        }
    }


    [HarmonyPatch(typeof(MaskedPlayerEnemy))]
    internal class MajoraMaskedPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("Update")]
        public static void UpdatePatch(MaskedPlayerEnemy __instance)
        {
            if (!Plugin.config.majoraWeather.Value || MajoraMaskItem.Instance == null)
                return;
            if (MajoraMaskItem.Instance.spawnedMajoraEnemy != null && !MajoraMaskItem.Instance.spawnedMajoraEnemy.isEnemyDead
                && MajoraMaskItem.Instance.spawnedMajoraEnemy.GetInstanceID() == __instance.GetInstanceID())
            {
                UpgradeSpeed(__instance, true);
            }
            else if (MajoraMaskItem.Instance.spawnedMaskedEnemies != null && MajoraMaskItem.Instance.spawnedMaskedEnemies.Count >= 1
                && MajoraMaskItem.Instance.spawnedMaskedEnemies.Any(x => x != null && !x.isEnemyDead && x.GetInstanceID() == __instance.GetInstanceID()))
            {
                UpgradeSpeed(__instance, false);
            }
        }

        private static void UpgradeSpeed(MaskedPlayerEnemy enemy, bool veryFast = false)
        {
            if (enemy.isEnemyDead || !enemy.enemyEnabled || !enemy.ventAnimationFinished || enemy.inSpecialAnimation)
                return;
            switch (enemy.currentBehaviourStateIndex)
            {
                case 0:
                    enemy.agent.speed += veryFast ? 2f : 1f;
                    break;
                case 1:
                    if (enemy.IsOwner && enemy.stopAndStareTimer < 0f)
                    {
                        if (enemy.running || enemy.runningRandomly)
                            enemy.agent.speed += veryFast ? 6f : 3f;
                        else
                            enemy.agent.speed += veryFast ? 1.2f : 0.6f;
                    }
                    break;
                default:
                    break;
            }
        }
    }
}