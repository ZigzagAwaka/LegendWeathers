using Unity.Netcode;
using UnityEngine;

namespace LegendWeathers.Weathers
{
    public class MajoraMoonWeather : LegendWeathers
    {
        public static WeatherInfo weatherInfo = new WeatherInfo("Majora Moon", 50, 1.8f, 1f, new Color(0.7f, 0f, 0.8f, 1f));
        private GameObject? spawnedMoon = null;

        public MajoraMoonWeather() : base(weatherInfo) { }

        public override void OnEnable()
        {
            base.OnEnable();
            if (!WeatherRegistry.WeatherManager.IsSetupFinished)
                return;
            if (!RoundManager.Instance.currentLevel.planetHasTime)
            {
                Plugin.logger.LogError(weatherInfo.name + " requires a planet with time.");
                return;
            }
            if (NetworkManager.Singleton.IsServer)
            {
                var position = MajoraMoonPositions.Get(RoundManager.Instance.currentLevel.PlanetName);
                spawnedMoon = Instantiate(Plugin.instance.majoraMoonObject, position.Item1, Quaternion.Euler(position.Item2));
                if (spawnedMoon != null)
                {
                    var moonNetObj = spawnedMoon.GetComponent<NetworkObject>();
                    moonNetObj.Spawn(true);
                    spawnedMoon.GetComponent<MajoraMoon>().InitializeMoonServerRpc(moonNetObj, position.Item3);
                }
                else
                    Plugin.logger.LogError("Failed to spawn " + weatherInfo.name + " on the server.");
            }
            EnableVanillaSun(false);
            EnableVanillaVolumeFog(false);
        }

        public override void OnDisable()
        {
            base.OnDisable();
            if (!WeatherRegistry.WeatherManager.IsSetupFinished)
                return;
            if (NetworkManager.Singleton.IsServer)
            {
                if (spawnedMoon != null)
                {
                    var networkObject = spawnedMoon.GetComponent<NetworkObject>();
                    if (networkObject != null && networkObject.IsSpawned)
                    {
                        networkObject.Despawn(true);
                    }
                    spawnedMoon = null;
                }
            }
            EnableVanillaSun(true);
            EnableVanillaVolumeFog(true);
        }
    }
}
