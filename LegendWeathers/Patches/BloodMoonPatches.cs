using HarmonyLib;
using LegendWeathers.Weathers;
using LegendWeathers.WeatherSkyEffects;
using Unity.Netcode;

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


    [HarmonyPatch(typeof(NutcrackerEnemyAI))]
    internal class BloodMoonNutcrackerPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch("SpawnShotgunShells")]
        public static bool SpawnShotgunShellsPatch(NutcrackerEnemyAI __instance)
        {
            return !(Plugin.config.bloodMoonWeather.Value && BloodMoonWeather.BloodMoonEffectReference != null && BloodMoonWeather.BloodMoonEffectReference.EffectEnabled
                && __instance != null && __instance.enemyType != null);
        }

        [HarmonyPrefix]
        [HarmonyPatch("DropGun")]
        public static bool DropGunPatch(NutcrackerEnemyAI __instance)
        {
            if (!Plugin.config.bloodMoonWeather.Value || BloodMoonWeather.BloodMoonEffectReference == null || !BloodMoonWeather.BloodMoonEffectReference.EffectEnabled
                || __instance == null || __instance.enemyType == null || __instance.gun == null)
            {
                return true;
            }
            if (!BloodMoonManager.hasFirstNutcrackerRespawned)
            {
                __instance.gun.shellsLoaded = 2;
                BloodMoonManager.hasFirstNutcrackerRespawned = true;
                return true;
            }
            else
            {
                if (__instance.IsServer)
                {
                    var gunNetwork = __instance.gun.gameObject.GetComponent<NetworkObject>();
                    if (gunNetwork != null && gunNetwork.IsSpawned)
                    {
                        gunNetwork.Despawn();
                    }
                }
                return false;
            }
        }
    }
}
