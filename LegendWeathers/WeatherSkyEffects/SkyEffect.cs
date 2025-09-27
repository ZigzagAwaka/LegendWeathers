using LegendWeathers.Utils;
using UnityEngine;
using UnityEngine.Rendering;

namespace LegendWeathers.WeatherSkyEffects
{
    public class SkyEffect : MonoBehaviour
    {
        internal readonly GameObject effectGameObject;
        internal readonly string effectName;
        internal bool fogVolumeComponentExists = false;

        private bool renderingSafeCheck = false;
        private float renderingSafeCheckTimer = 0f;

        public GameObject? spawnedSky = null;
        public Volume? spawnedSkyVolume = null;
        public bool overrideFogVolume = false;
        public bool IsEffectActive { get; internal set; } = false;

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
                Effects.EnableVanillaVolumeFog(false, ref fogVolumeComponentExists);
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
                Effects.EnableVanillaVolumeFog(true, ref fogVolumeComponentExists);
        }

        private void SetupSkyVolume()
        {
            if (Effects.SetupCustomSkyVolume(spawnedSky, out spawnedSkyVolume))
                IsEffectActive = true;
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
    }
}
