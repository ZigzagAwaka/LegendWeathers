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

        private bool renderingSafeCheck = false;
        private float renderingSafeCheckTimer = 0f;

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
                SetupSkyVolume();
                if (!IsEffectActive)
                    Plugin.logger.LogWarning("Failed to setup fog component in " + effectName + " Sky Effect Volume. The effect could be rendered incorrectly.");
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

        private void SetupSkyVolume()
        {
            if (spawnedSky == null)
                return;
            spawnedSkyVolume = spawnedSky.GetComponent<Volume>();
            foreach (var component in spawnedSkyVolume.profile.components)
            {
                if (component.active && component is Fog fog)
                {
                    fog.enableVolumetricFog.value = false;
                    fog.enableVolumetricFog.overrideState = true;
                    IsEffectActive = true;
                    return;
                }
            }
        }


        public virtual void Update()
        {
            PerformRenderingSafeCheck();
        }

        // Perform a safety check to ensure the effect is correctly rendered
        private void PerformRenderingSafeCheck()
        {
            if (!IsEffectActive && spawnedSky != null && !renderingSafeCheck)
            {
                renderingSafeCheckTimer += Time.deltaTime;
                if (renderingSafeCheckTimer > 1f)
                {
                    SetupSkyVolume();
                    if (IsEffectActive)
                    {
                        renderingSafeCheck = true;
                        renderingSafeCheckTimer = 0f;
                    }
                    else
                        Plugin.logger.LogError("Failed to perform the rendering safe check in " + effectName + " Sky Effect Volume.");
                }
            }
        }

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
