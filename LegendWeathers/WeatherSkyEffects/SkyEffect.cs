using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace LegendWeathers.WeatherSkyEffects
{
    public class SkyEffect : MonoBehaviour
    {
        private readonly GameObject effectGameObject;
        private readonly string effectName;

        public GameObject? spawnedSky = null;
        public Volume? spawnedSkyVolume = null;
        public bool isEffectReady = false;

        public SkyEffect(GameObject? gameObject)
        {
            if (gameObject == null)
            {
                throw new System.NullReferenceException("SkyEffect GameObject is null.");
            }
            effectGameObject = gameObject;
            effectName = gameObject.name;
        }

        public virtual void OnEnable()
        {
            if (!WeatherRegistry.WeatherManager.IsSetupFinished)
                return;
            spawnedSky = Instantiate(effectGameObject);
            if (spawnedSky != null)
            {
                spawnedSkyVolume ??= spawnedSky.GetComponent<Volume>();
                foreach (var component in spawnedSkyVolume.profile.components)
                {
                    if (component.active && component is Fog fog)
                    {
                        fog.enableVolumetricFog.value = false;
                        fog.enableVolumetricFog.overrideState = true;
                        isEffectReady = true;
                        break;
                    }
                }
            }
            else
                Plugin.logger.LogError("Failed to instantiate " + effectName + " Sky Effect.");
        }

        public virtual void OnDisable()
        {
            if (!WeatherRegistry.WeatherManager.IsSetupFinished)
                return;
            if (spawnedSky != null)
            {
                isEffectReady = false;
                Destroy(spawnedSky);
                spawnedSky = null;
                spawnedSkyVolume = null;
            }
        }

        public virtual void Update() { }
    }
}
