using LegendWeathers.Utils;
using LegendWeathers.Weathers;
using System.Collections.Generic;
using UnityEngine;

namespace LegendWeathers.WeatherSkyEffects
{
    public class BloodSkyEffect : SunEffect
    {
        private readonly int maxSpawnedEffectObjects = 30;
        private readonly List<GameObject> spawnedEffectsObjects = new List<GameObject>();
        private readonly List<Vector3> effectObjectsPositions = new List<Vector3>();

        private readonly bool eclipsedMusicEnabled = false;
        private GameObject? eclipsedObjectMusic;

        public readonly static float bloodMoonSunAnimatorFixedTime = 0.15f;

        public BloodSkyEffect() : base(Plugin.instance.bloodSunObject)
        {
            modifySunlightColor = true;
            sunlightColor = new Color(1, 0.36f, 0.7f);
            sunSizeFactor = Plugin.config.bloodMoonSizeFactor.Value;
            maxSpawnedEffectObjects = Plugin.config.bloodMoonEffectsAbundance.Value;
            eclipsedMusicEnabled = Plugin.config.bloodMoonAmbienceMusicType.Value == "Eclipsed";
        }

        public override void OnEnable()
        {
            if (WeatherRegistry.WeatherManager.IsSetupFinished)
            {
                base.OnEnable();
                if (eclipsedMusicEnabled)
                {
                    ActivateEclipsedMusic(true);
                }
                if (IsEffectActive && (Plugin.config.bloodMoonAmbienceMusicVolume.Value != 1f || eclipsedMusicEnabled))
                {
                    CustomizeAmbientMusicVolume();
                }
                if (effectObjectsPositions.Count == 0)
                {
                    for (int i = 0; i < maxSpawnedEffectObjects; i++)
                    {
                        effectObjectsPositions.Add(Effects.GetArbitraryMoonPosition(i, maxSpawnedEffectObjects, radiusModifier: 10));
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
                if (eclipsedMusicEnabled)
                {
                    ActivateEclipsedMusic(false);
                }
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
            eclipsedObjectMusic = null;
        }

        public static void CheckAndReplaceMaterial()
        {
            var bloodSun = Plugin.instance.bloodSunObject;
            var bloodManager = Plugin.instance.bloodMoonManagerObject;
            if (bloodSun == null || bloodManager == null)
            {
                Plugin.logger.LogError("Failed to replace the Blood Moon texture, blood sun is null or blood manager is null.");
                return;
            }
            bloodSun.GetComponent<MeshRenderer>().material = bloodManager.GetComponent<BloodMoonManager>().bloodMoonTextureMaterials[1];
        }

        private void ActivateEclipsedMusic(bool activate)
        {
            if (eclipsedObjectMusic == null)
            {
                eclipsedObjectMusic = FindObjectOfType<EclipseWeather>(true).gameObject;
            }
            eclipsedObjectMusic.SetActive(activate);
        }

        private void CustomizeAmbientMusicVolume()
        {
            if (spawnedSun != null)
            {
                foreach (var audio in spawnedSun.GetComponentsInChildren<AudioSource>())
                {
                    if (audio != null)
                        audio.volume = eclipsedMusicEnabled ? 0f : Plugin.config.bloodMoonAmbienceMusicVolume.Value;
                }
            }
        }
    }
}