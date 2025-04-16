using LegendWeathers.BehaviourScripts;
using Unity.Netcode;
using UnityEngine;

namespace LegendWeathers.Weathers
{
    public class MajoraMoonWeather : LegendWeather
    {
        public static WeatherInfo weatherInfo = new WeatherInfo("Majora Moon", 50, 1.7f, 1f, new Color(0.7f, 0f, 0.8f, 1f));
        public static string weatherAlert = "The grimacing moon moves inexorably closer. Be quick!";
        private GameObject? spawnedMoon = null;
        private GameObject? spawnedMask = null;

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
                {
                    Plugin.logger.LogError("Failed to spawn " + weatherInfo.name + " on the server.");
                }
                if (Plugin.instance.majoraMaskItem != null)
                {
                    var maskPosition = RoundManager.Instance.insideAINodes[Random.Range(0, RoundManager.Instance.insideAINodes.Length - 1)].transform.position;
                    spawnedMask = Instantiate(Plugin.instance.majoraMaskItem.spawnPrefab, maskPosition + Vector3.up * 0.25f, Quaternion.identity, RoundManager.Instance.spawnedScrapContainer);
                    if (spawnedMask != null)
                    {
                        var maskComponent = spawnedMask.GetComponent<MajoraMaskItem>();
                        maskComponent.transform.rotation = Quaternion.Euler(maskComponent.itemProperties.restingRotation);
                        maskComponent.fallTime = 1f;
                        maskComponent.hasHitGround = true;
                        maskComponent.reachedFloorTarget = true;
                        maskComponent.isInFactory = true;
                        maskComponent.scrapValue = (int)(Random.Range(Plugin.instance.majoraMaskItem.minValue, Plugin.instance.majoraMaskItem.maxValue) * RoundManager.Instance.scrapValueMultiplier);
                        maskComponent.NetworkObject.Spawn();
                        maskComponent.SyncMaskServerRpc(maskComponent.NetworkObject, maskComponent.scrapValue);
                    }
                    else
                    {
                        Plugin.logger.LogError("Failed to spawn the Majora Mask item on the server.");
                    }
                }
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
                if (spawnedMask != null)
                {
                    var maskComponent = spawnedMask.GetComponent<MajoraMaskItem>();
                    var networkObject = spawnedMask.GetComponent<NetworkObject>();
                    if (maskComponent != null && !maskComponent.hasBeenFound)
                    {
                        if (networkObject != null && networkObject.IsSpawned)
                        {
                            networkObject.Despawn(true);
                        }
                        spawnedMask = null;
                    }
                }
            }
            EnableVanillaSun(true);
            EnableVanillaVolumeFog(true);
        }
    }
}
