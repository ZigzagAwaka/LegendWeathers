using HarmonyLib;
using LegendWeathers.BehaviourScripts;
using System.Linq;

namespace LegendWeathers.Utils
{
    [HarmonyPatch(typeof(StartOfRound))]
    internal class WeatherAlertPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("OnShipLandedMiscEvents")]
        public static void OnShipLandedMiscEventsPatch()
        {
            // WILL CAUSE ISSUES UNTIL VANILLA IS FIXED
            /*if (Effects.IsWeatherEffectPresent("majoramoon"))
            {
                Effects.MessageOneTime("Weather alert!", MajoraMoonWeather.weatherAlert, true, "LW_MajoraTip");
            }*/
        }
    }


    [HarmonyPatch(typeof(MaskedPlayerEnemy))]
    internal class MajoraMaskedPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("Update")]
        public static void UpdatePatch(MaskedPlayerEnemy __instance)
        {
            if (MajoraMaskItem.Instance == null)
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