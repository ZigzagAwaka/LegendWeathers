using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace LegendWeathers.WeatherSkyEffects
{
    public class SkyEffect : MonoBehaviour
    {
        private readonly GameObject effectGameObject;
        private readonly string effectName;
        private bool fogVolumeComponentExists = false;

        public GameObject? spawnedSky = null;
        public Volume? spawnedSkyVolume = null;
        public bool overrideFogVolume = false;
        public bool IsEffectActive { get; private set; } = false;


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
                        IsEffectActive = true;
                        break;
                    }
                }
            }
            else
                Plugin.logger.LogError("Failed to instantiate " + effectName + " Sky Effect.");
            if (overrideFogVolume)
                EnableVanillaVolumeFog(false);
        }

        public virtual void OnDisable()
        {
            if (!WeatherRegistry.WeatherManager.IsSetupFinished)
                return;
            if (spawnedSky != null)
            {
                IsEffectActive = false;
                Destroy(spawnedSky);
                spawnedSky = null;
                spawnedSkyVolume = null;
            }
            if (overrideFogVolume)
                EnableVanillaVolumeFog(true);
        }

        public virtual void Update() { }

        private void EnableVanillaVolumeFog(bool enabled)
        {
            try
            {
                if (enabled && !fogVolumeComponentExists)
                    return;
                foreach (var volume in FindObjectsOfType<Volume>())
                {
                    if (volume == null || volume.profile == null || volume.gameObject.scene.name != RoundManager.Instance.currentLevel.sceneName)
                        continue;
                    foreach (var component in volume.profile.components)
                    {
                        if (component.active && component is Fog)
                        {
                            component.active = enabled;
                            fogVolumeComponentExists = !enabled;
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                Plugin.logger.LogError("Failed to " + (enabled ? "enable" : "disable") + " vanilla volume fog.\n" + e.Message);
            }
        }
    }
}
