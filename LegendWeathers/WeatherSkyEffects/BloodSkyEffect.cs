using LegendWeathers.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace LegendWeathers.WeatherSkyEffects
{
    public class BloodSkyEffect : SunEffect
    {
        private readonly int maxSpawnedEffectObjects = 30;
        private readonly List<GameObject> spawnedEffectsObjects = new List<GameObject>();
        private readonly List<Vector3> effectObjectsPositions = new List<Vector3>();

        public readonly static float bloodMoonSunAnimatorFixedTime = 0.15f;

        public BloodSkyEffect() : base(Plugin.instance.bloodSunObject)
        {
            modifySunlightColor = true;
            sunlightColor = new Color(1, 0.36f, 0.7f);
            sunSizeFactor = Plugin.config.bloodMoonSizeFactor.Value;
            maxSpawnedEffectObjects = Plugin.config.bloodMoonEffectsAbundance.Value;
        }

        public override void OnEnable()
        {
            if (WeatherRegistry.WeatherManager.IsSetupFinished)
            {
                base.OnEnable();
                if (effectObjectsPositions.Count == 0)
                {
                    for (int i = 0; i < maxSpawnedEffectObjects; i++)
                    {
                        effectObjectsPositions.Add(Effects.GetArbitraryMoonPosition(i, maxSpawnedEffectObjects));
                    }
                }
                if (effectObjectsPositions.Count != 0)
                {
                    bool swapEffect = false;
                    foreach (var position in effectObjectsPositions)
                    {
                        var terrainObject = Instantiate(Plugin.instance.bloodTerrainEffectObject, position, Quaternion.identity);
                        if (terrainObject != null)
                        {
                            if (swapEffect)
                            {
                                terrainObject.transform.Find("Flame").gameObject.SetActive(false);
                                terrainObject.transform.Find("BlackFog").gameObject.SetActive(true);
                            }
                            spawnedEffectsObjects.Add(terrainObject);
                            swapEffect = !swapEffect;
                        }
                    }
                }
            }
        }

        public override void OnDisable()
        {
            if (WeatherRegistry.WeatherManager.IsSetupFinished)
            {
                base.OnDisable();
                foreach (var terrainObject in spawnedEffectsObjects)
                {
                    if (terrainObject != null)
                        Destroy(terrainObject);
                }
                spawnedEffectsObjects.Clear();
            }
        }

        public void ResetState()
        {
            effectObjectsPositions.Clear();
        }

        public static void CheckAndReplaceTexture()
        {
            var bloodSun = Plugin.instance.bloodSunObject;
            if (bloodSun == null)
            {
                Plugin.logger.LogError("Failed to replace the Blood Moon texture, blood sun is null.");
                return;
            }
            bloodSun.transform.Find("Classic").gameObject.SetActive(false);
            bloodSun.transform.Find("Bright").gameObject.SetActive(true);
        }
    }
}