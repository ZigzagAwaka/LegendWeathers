using LegendWeathers.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace LegendWeathers.WeatherSkyEffects
{
    public class BloodSkyEffect : SkyEffect
    {
        public GameObject? spawnedBloodSun = null;
        public GameObject? spawnedBloodParticles = null;
        public List<GameObject> spawnedEffectsObjects = new List<GameObject>();

        private readonly float bloodSunSizeFactor = 5.5f;
        private readonly Vector3 bloodParticleOffset = Vector3.up * 7f;
        private readonly int maxSpawnedEffectObjects = 20;
        private readonly List<Vector3> effectObjectsPositions = new List<Vector3>();

        private readonly List<Light> originalSunlights = new List<Light>();
        private readonly List<Color> originalSunlightColors = new List<Color>();
        private readonly Color bloodSunlightColor = new Color(1, 0.36f, 0.7f);

        public readonly static float bloodMoonSunAnimatorFixedTime = 0.15f;

        public BloodSkyEffect() : base(Plugin.instance.bloodSkyObject)
        {
            overrideFogVolume = true;
            bloodSunSizeFactor = Plugin.config.bloodMoonSizeFactor.Value;
            maxSpawnedEffectObjects = Plugin.config.bloodMoonEffectsAbundance.Value;
        }

        public override void OnEnable()
        {
            if (WeatherRegistry.WeatherManager.IsSetupFinished)
            {
                base.OnEnable();
                var sunAnim = FindObjectOfType<animatedSun>();
                if (sunAnim != null)
                {
                    foreach (var light in sunAnim.gameObject.GetComponentsInChildren<Light>())
                    {
                        originalSunlights.Add(light);
                        originalSunlightColors.Add(light.color);
                        light.color = bloodSunlightColor;
                    }
                    var sunTextureTransform = sunAnim.transform.Find("SunTexture");
                    if (sunTextureTransform != null)
                    {
                        spawnedBloodSun = Instantiate(Plugin.instance.bloodSunObject, sunTextureTransform.position, sunTextureTransform.rotation, sunTextureTransform.parent);
                        if (spawnedBloodSun != null)
                        {
                            spawnedBloodSun.transform.localScale = sunTextureTransform.localScale * bloodSunSizeFactor;
                        }
                        else
                            Plugin.logger.LogError("Failed to instantiate SunBloodTexture.");
                    }
                    else
                        Plugin.logger.LogError("Failed to find SunTexture transform in the actual moon scene.");
                }
                else
                {
                    Plugin.logger.LogError("Failed to find AnimatedSun component in the actual moon scene.");
                }
                SpawnExteriorEffects();
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
                if (spawnedBloodParticles != null)
                    Destroy(spawnedBloodParticles);
                if (spawnedBloodSun != null)
                    Destroy(spawnedBloodSun);
                for (var i = 0; i < originalSunlights.Count; i++)
                {
                    if (originalSunlights[i] != null)
                        originalSunlights[i].color = originalSunlightColors[i];
                }
                originalSunlights.Clear();
                originalSunlightColors.Clear();
            }
        }

        private void SpawnExteriorEffects()
        {
            var player = Effects.GetLocalPlayerAbsolute();
            if (player != null)
            {
                spawnedBloodParticles = Instantiate(Plugin.instance.bloodParticlesObject, player.transform.position + bloodParticleOffset, Quaternion.identity);
                if (spawnedBloodParticles == null)
                    Plugin.logger.LogError("Failed to instantiate BloodParticles for player " + player.playerUsername + ".");
            }
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

        public override void Update()
        {
            base.Update();
            if (spawnedBloodParticles != null)
            {
                var player = Effects.GetLocalPlayerAbsolute();
                if (player != null)
                {
                    spawnedBloodParticles.transform.position = player.transform.position + bloodParticleOffset;
                }
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