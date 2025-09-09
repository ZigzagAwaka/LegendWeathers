using LegendWeathers.WeatherSkyEffects;
using Unity.Netcode;
using UnityEngine;
using WeatherRegistry;

namespace LegendWeathers.Weathers
{
    public class BloodMoonWeather : LegendWeather
    {
        public static string weatherAlert = "";
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
            EnableVanillaSun(true);
        }
    }
}
