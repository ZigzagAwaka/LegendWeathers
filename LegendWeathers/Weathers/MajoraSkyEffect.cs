using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace LegendWeathers.Weathers
{
    public class MajoraSkyEffect : MonoBehaviour
    {
        private GameObject? spawnedSky = null;
        private Volume? skyVolume = null;
        private bool isReady = false;
        private readonly float maxWeight = 0.9f;

        public void OnEnable()
        {
            if (!WeatherRegistry.WeatherManager.IsSetupFinished || !RoundManager.Instance.currentLevel.planetHasTime)
                return;
            spawnedSky = Instantiate(Plugin.instance.majoraSkyObject);
            if (spawnedSky != null)
            {
                skyVolume ??= spawnedSky.GetComponent<Volume>();
                foreach (var component in skyVolume.profile.components)
                {
                    if (component.active && component is Fog fog)
                    {
                        fog.enableVolumetricFog.value = false;
                        fog.enableVolumetricFog.overrideState = true;
                        isReady = true;
                        break;
                    }
                }
            }
            else
                Plugin.logger.LogError("Failed to instantiate Majora sky.");
        }

        public void OnDisable()
        {
            if (!WeatherRegistry.WeatherManager.IsSetupFinished)
                return;
            if (spawnedSky != null)
            {
                isReady = false;
                Destroy(spawnedSky);
                spawnedSky = null;
                skyVolume = null;
            }
        }

        public void Update()
        {
            if (isReady && skyVolume != null)
            {
                if (TimeOfDay.Instance.currentDayTimeStarted && skyVolume.weight < maxWeight)
                {
                    var endTime = TimeOfDay.Instance.globalTimeAtEndOfDay / 2.1f;
                    skyVolume.weight = TimeOfDay.Instance.currentDayTime * maxWeight / endTime;
                }
            }
        }
    }
}
