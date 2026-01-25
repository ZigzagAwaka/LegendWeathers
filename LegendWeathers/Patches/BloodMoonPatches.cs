using HarmonyLib;
using LegendWeathers.Utils;
using LegendWeathers.Weathers;
using LegendWeathers.WeatherSkyEffects;

namespace LegendWeathers.Patches
{
    [HarmonyPatch]
    internal class BloodMoonPatches
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(TimeOfDay), "MoveTimeOfDay")]
        public static void BloodMoonFixedTime(TimeOfDay __instance)
        {
            if (Plugin.config.bloodMoonWeather.Value && __instance.sunAnimator != null &&
                BloodMoonWeather.BloodMoonEffectReference != null && BloodMoonWeather.BloodMoonEffectReference.EffectActive)
            {
                __instance.sunAnimator.SetFloat("timeOfDay", BloodSkyEffect.bloodMoonSunAnimatorFixedTime);
            }
        }


        [HarmonyPostfix]
        [HarmonyPatch(typeof(EnemyAI), "KillEnemy")]
        public static void TryEnemyResurrection(EnemyAI __instance, bool destroy)
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


        [HarmonyPrefix]
        [HarmonyPatch(typeof(NutcrackerEnemyAI), "SpawnShotgunShells")]
        public static bool NutcrackerShellsSkip(NutcrackerEnemyAI __instance)
        {
            if (Plugin.config.bloodMoonSpecificEnemiesItemSpawningMode.Value == "Chance")
            {
                return Compatibility.ManageSpecialItemForBloodMoonByChance(null);
            }
            return !(Plugin.config.bloodMoonWeather.Value && BloodMoonWeather.BloodMoonEffectReference != null && BloodMoonWeather.BloodMoonEffectReference.EffectEnabled
                && __instance != null && __instance.enemyType != null);
        }


        [HarmonyPrefix]
        [HarmonyPatch(typeof(NutcrackerEnemyAI), "DropGun")]
        public static bool NutcrackerDropGunUnique(NutcrackerEnemyAI __instance)
        {
            if (!Plugin.config.bloodMoonWeather.Value || BloodMoonWeather.BloodMoonEffectReference == null || !BloodMoonWeather.BloodMoonEffectReference.EffectEnabled
                || __instance == null || __instance.enemyType == null || __instance.gun == null)
            {
                return true;
            }
            return Compatibility.ManageSpecialItemForBloodMoon("Nutcracker", __instance.gun);
        }
    }


    [HarmonyPatch]
    internal class BloodMoonHauntedHarpistPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(LethalCompanyHarpGhost.EnforcerGhost.EnforcerGhostAIClient), "HandleDropShotgun")]
        public static bool EnforcerGhostDropGunUnique(LethalCompanyHarpGhost.EnforcerGhost.EnforcerGhostAIClient __instance)
        {
            if (!Plugin.config.bloodMoonWeather.Value || BloodMoonWeather.BloodMoonEffectReference == null || !BloodMoonWeather.BloodMoonEffectReference.EffectEnabled
                || __instance == null || __instance._heldShotgun == null)
            {
                return true;
            }
            return Compatibility.ManageSpecialItemForBloodMoon("EnforcerGhost", __instance._heldShotgun);
        }


        [HarmonyPrefix]
        [HarmonyPatch(typeof(LethalCompanyHarpGhost.BagpipesGhost.BagpipesGhostAIClient), "HandleDropInstrument")]
        public static bool BagpipesGhostDropInstrumentReduced(LethalCompanyHarpGhost.BagpipesGhost.BagpipesGhostAIClient __instance)
        {
            if (!Plugin.config.bloodMoonWeather.Value || BloodMoonWeather.BloodMoonEffectReference == null || !BloodMoonWeather.BloodMoonEffectReference.EffectEnabled
                || __instance == null || __instance._heldInstrument == null)
            {
                return true;
            }
            return Compatibility.ManageSpecialItemForBloodMoon("BagpipesGhost", __instance._heldInstrument);
        }


        [HarmonyPrefix]
        [HarmonyPatch(typeof(LethalCompanyHarpGhost.HarpGhost.HarpGhostAIClient), "HandleDropInstrument")]
        public static bool HarpGhostDropInstrumentReduced(LethalCompanyHarpGhost.HarpGhost.HarpGhostAIClient __instance)
        {
            if (!Plugin.config.bloodMoonWeather.Value || BloodMoonWeather.BloodMoonEffectReference == null || !BloodMoonWeather.BloodMoonEffectReference.EffectEnabled
                || __instance == null || __instance._heldInstrument == null)
            {
                return true;
            }
            return Compatibility.ManageSpecialItemForBloodMoon("HarpGhost", __instance._heldInstrument);
        }
    }
}
