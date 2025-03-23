using Unity.Netcode;
using UnityEngine;

namespace LegendWeathers.Weathers
{
    public class MajoraMoonWeather : LegendWeathers
    {
        public static WeatherInfo weatherInfo = new WeatherInfo("Majora Moon", 30, 1.6f, 1.2f, new Color(0.7f, 0f, 0.15f, 1f));
        private GameObject? spawnedMoon = null;

        public MajoraMoonWeather() : base(weatherInfo) { }

        public override void OnEnable()
        {
            base.OnEnable();
            if (!WeatherRegistry.WeatherManager.IsSetupFinished)
                return;
            if (NetworkManager.Singleton.IsServer)
            {
                var position = MajoraMoonPositions.Get(RoundManager.Instance.currentLevel.PlanetName);
                spawnedMoon = Instantiate(Plugin.instance.majoraMoonObject, position.Item1, Quaternion.Euler(position.Item2));
                spawnedMoon?.GetComponent<NetworkObject>().Spawn(true);
            }
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
        }
    }
}
