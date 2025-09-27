using LegendWeathers.WeatherSkyEffects;
using Unity.Netcode;
using UnityEngine;
using WeatherRegistry;

namespace LegendWeathers.Weathers
{
    public class BloodMoonWeather : LegendWeather
    {
        public static string weatherAlert = "A crimson moon rises, bathing the land in danger. Be on your guard.";
        private GameObject? spawnedManager = null;

        public static ImprovedWeatherEffect? BloodMoonEffectReference { get; private set; } = null;

        public BloodMoonWeather() : base(Plugin.instance.bloodMoonDefinition) { }

        public override void OnEnable()
        {
            base.OnEnable();
            if (!WeatherManager.IsSetupFinished)
                return;
            if (NetworkManager.Singleton.IsServer)
            {
                spawnedManager = Instantiate(Plugin.instance.bloodMoonManagerObject, Vector3.zero, Quaternion.identity);
                if (spawnedManager != null)
                {
                    var managerNetObj = spawnedManager.GetComponent<NetworkObject>();
                    managerNetObj.Spawn(true);
                    spawnedManager.GetComponent<BloodMoonManager>().InitializeManagerServerRpc(managerNetObj);
                }
                else
                {
                    Plugin.logger.LogError("Failed to spawn " + weatherDefinition.Name + " on the server.");
                }
            }
            BloodMoonEffectReference = ConfigHelper.ResolveStringToWeather("bloodmoon").Effect;
            UpdateSpawningVariables(true);
            EnableVanillaSun(false);
        }

        public override void OnDisable()
        {
            base.OnDisable();
            if (!WeatherManager.IsSetupFinished)
                return;
            if (NetworkManager.Singleton.IsServer)
            {
                if (spawnedManager != null)
                {
                    var managerNetObj = spawnedManager.GetComponent<NetworkObject>();
                    if (managerNetObj != null && managerNetObj.IsSpawned)
                    {
                        managerNetObj.Despawn(true);
                    }
                    spawnedManager = null;
                }
            }
            BloodMoonEffectReference?.EffectObject?.GetComponent<BloodSkyEffect>()?.ResetState();
            BloodMoonEffectReference = null;
            UpdateSpawningVariables(false);
            EnableVanillaSun(true);
        }

        private void UpdateSpawningVariables(bool update)
        {
            var currentLevel = RoundManager.Instance.currentLevel;
            if (currentLevel != null)
            {
                var minEnemies = update ? GetDifficultyIndex() : 0;
                RoundManager.Instance.minOutsideEnemiesToSpawn = minEnemies;
                RoundManager.Instance.minEnemiesToSpawn = minEnemies;
                currentLevel.maxEnemyPowerCount += update ? 5 : -5;
                currentLevel.maxOutsideEnemyPowerCount += update ? 10 : -10;
                //currentLevel.maxDaytimeEnemyPowerCount = 0;
            }
        }

        private int GetDifficultyIndex()
        {
            var difficulty = Plugin.config.bloodMoonDifficultyFactor.Value;
            return difficulty == "Easy" ? 0 : (difficulty == "Hard" ? 2 : 1);
        }

        public BloodMoonManager? GetBloodMoonManager()
        {
            if (spawnedManager != null)
            {
                return spawnedManager.GetComponent<BloodMoonManager>();
            }
            return null;
        }
    }
}
