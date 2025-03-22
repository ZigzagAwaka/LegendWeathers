using Unity.Netcode;
using UnityEngine;

namespace LegendWeathers.Weathers
{
    public class MajoraMoonWeather : LegendWeathers
    {
        public static WeatherInfo weatherInfo = new WeatherInfo("Majora", 30, 1.6f, 1.2f, new Color(1f, 0.2f, 0.2f, 1f));
        private GameObject? spawnedMoon = null;

        public MajoraMoonWeather() : base(weatherInfo) { }

        public override void OnEnable()
        {
            base.OnEnable();
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
            {
                spawnedMoon = Instantiate(Plugin.instance.majoraMoonObject, RoundManager.Instance.outsideAINodes[0].transform.position, Quaternion.Euler(0f, 0f, 0f));
                spawnedMoon?.GetComponent<NetworkObject>().Spawn(true);
            }
        }

        public override void OnDisable()
        {
            base.OnDisable();
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
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
