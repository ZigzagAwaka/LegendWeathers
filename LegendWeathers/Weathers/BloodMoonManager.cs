using GameNetcodeStuff;
using LegendWeathers.BehaviourScripts;
using LegendWeathers.Utils;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

namespace LegendWeathers.Weathers
{
    public class BloodMoonManager : NetworkBehaviour
    {
        public AudioSource introMusicAudio = null!;
        public AudioSource sfxAudio = null!;
        public AudioClip[] sfx = null!;
        public GameObject bloodMoonVolumeObject = null!;
        public GameObject bloodMoonParticlesObject = null!;
        public GameObject resurrectEnemyInvocationObject = null!;
        public GameObject resurrectEnemyBurstObject = null!;
        public Material[] bloodMoonTextureMaterials = null!;

        private bool isInitialized = false;
        private PlayerControllerB? localPlayer = null;
        private bool localPlayerIsInsideLastChecked = false;

        private GameObject? spawnedBloodVolume = null;
        private bool volumeCorrectlyRendered = false;
        private float volumeRenderingSafeCheckTimer = 0f;
        private bool vanillaFogVolumeComponentExists = false;

        private GameObject? spawnedBloodParticles = null;
        private readonly Vector3 bloodParticleOffset = Vector3.up * 7f;

        private float lastMoonSfxTime = 0;
        private float nextMoonSfxTime = 63;  // 63 = initial delay outside before first sfx
        private readonly float moonSfxTimeIntervalInside = 10.8f;
        private readonly float moonSfxTimeIntervalOutside = 81.8f;

        private float lastThunderEventTime = 0;
        private float nextThunderEventTime = 60f;

        private List<VisualEnvironment> visualEnvironments = new List<VisualEnvironment>();
        private List<float> originalWindSpeeds = new List<float>();
        private readonly int windSpeedFactor = 4;

        // To keep track of special enemies that have already respawned once
        // Only used when the config "Specific enemies item spawning Mode" is set to "Limited"
        private static readonly Dictionary<string, bool> firstEnemiesRespawned = new Dictionary<string, bool>();
        // Seed and next value for specific enemy item spawning
        // Only used when the config "Specific enemies item spawning Mode" is set to "Chance"
        private static System.Random? globalSeedForSpecificEnemyItemSpawning;
        public static int nextRandomSpecificEnemyItemSpawnChance = 0;


        public void Update()
        {
            if (!isInitialized)
                return;
            PerformRenderingSafeCheck();
            Effects.StopVanillaMusic();
            Effects.SetObjectPositionToLocalPlayer(spawnedBloodParticles, bloodParticleOffset);
            TryAndPlayThunderEvent();
            if (introMusicAudio.isPlaying)
                CheckIntroMusicState();
            else
                TryAndPlayMoonSfx();
        }

        private void CheckIntroMusicState()
        {
            if (localPlayer == null || localPlayer.isPlayerDead || localPlayer.isInsideFactory)
            {
                introMusicAudio.Stop();
            }
        }

        private void TryAndPlayMoonSfx()
        {
            if (localPlayer == null)
                return;

            if (Effects.IsPlayerInsideFacilityAbsolute(localPlayer))
            {
                if (!localPlayerIsInsideLastChecked)
                {
                    lastMoonSfxTime = 0;
                    nextMoonSfxTime = moonSfxTimeIntervalInside;
                }
                localPlayerIsInsideLastChecked = true;
            }
            else
            {
                localPlayerIsInsideLastChecked = false;
            }

            if (localPlayerIsInsideLastChecked)
                return;

            lastMoonSfxTime += Time.deltaTime;
            if (lastMoonSfxTime >= nextMoonSfxTime)
            {
                lastMoonSfxTime = 0;
                nextMoonSfxTime = moonSfxTimeIntervalOutside;
                if (Plugin.config.bloodMoonAmbientMusicType.Value == "Blood Moon")
                    sfxAudio.PlayOneShot(sfx[Random.Range(0, sfx.Length)], 0.5f * Plugin.config.bloodMoonAmbientMusicVolume.Value);
            }
        }

        private void TryAndPlayThunderEvent()
        {
            if (!IsServer)
                return;
            lastThunderEventTime += Time.deltaTime;
            if (lastThunderEventTime >= nextThunderEventTime)
            {
                lastThunderEventTime = 0;
                nextThunderEventTime = Random.Range(50f, 70f);
                if (Random.Range(0, 10) == 0)
                    SpawnBloodStone();
            }
        }

        private void SpawnBloodStone(Vector3? originalPosition = null)
        {
            if (Plugin.instance.bloodStoneItem == null || !Plugin.config.bloodMoonStoneItemSpawning.Value)
                return;
            Vector3 spawnPosition = originalPosition == null ? Effects.GetRandomMoonPosition() : RoundManager.Instance.GetRandomNavMeshPositionInRadius((Vector3)originalPosition, 15);
            if (originalPosition == null)
            {
                Effects.SpawnBloodLightningBolt(ref spawnPosition);
                SpawnLightningClientRpc(spawnPosition);
            }
            var bloodStone = Instantiate(Plugin.instance.bloodStoneItem.spawnPrefab, spawnPosition, Quaternion.identity, RoundManager.Instance.spawnedScrapContainer);
            if (bloodStone != null)
            {
                var item = bloodStone.GetComponent<BloodStoneItem>();
                item.transform.rotation = Quaternion.Euler(item.itemProperties.restingRotation);
                item.fallTime = 1f;
                item.hasHitGround = true;
                item.reachedFloorTarget = true;
                item.scrapValue = (int)(Random.Range(Plugin.instance.bloodStoneItem.minValue, Plugin.instance.bloodStoneItem.maxValue) * RoundManager.Instance.scrapValueMultiplier);
                if (originalPosition == null)
                    item.scrapValue *= 4;
                item.NetworkObject.Spawn();
                item.StartInvocationServerRpc(item.NetworkObject, item.scrapValue, isFast: originalPosition == null);
            }
            else
            {
                Plugin.logger.LogError("Failed to spawn the Blood Stone item on the server.");
            }
        }

        [ClientRpc]
        private void SpawnLightningClientRpc(Vector3 position)
        {
            if (!IsServer)
                Effects.SpawnBloodLightningBolt(ref position);
        }

        public void ResurrectEnemy(EnemyAI enemy)
        {
            if (IsServer)
                StartCoroutine(StartAnimationThenResurrect(enemy, enemy.transform.position));
        }

        private IEnumerator StartAnimationThenResurrect(EnemyAI enemy, Vector3 originalPosition)
        {
            if (enemy == null || Plugin.config.bloodMoonStoneSpawnBlacklist.Count == 0 || !Plugin.config.bloodMoonStoneSpawnBlacklist.Contains(enemy.enemyType.enemyName.ToLower()))
            {
                SpawnBloodStone(originalPosition);
            }
            if (Plugin.config.bloodMoonSpecificEnemiesItemSpawningMode.Value == "Chance" && globalSeedForSpecificEnemyItemSpawning != null)
            {
                nextRandomSpecificEnemyItemSpawnChance = globalSeedForSpecificEnemyItemSpawning.Next(0, 100);
            }
            yield return new WaitForSeconds(Plugin.config.bloodMoonResurrectWaitTime.Value);
            if (enemy == null || StartOfRound.Instance.inShipPhase || StartOfRound.Instance.shipIsLeaving || (Plugin.config.bloodMoonResurrectBlacklist.Count != 0 && Plugin.config.bloodMoonResurrectBlacklist.Contains(enemy.enemyType.enemyName.ToLower())))
                yield break;
            var spawnPosition = RoundManager.Instance.GetNavMeshPosition(originalPosition, sampleRadius: 3f);
            if (!RoundManager.Instance.GotNavMeshPositionResult)
                spawnPosition = Effects.GetClosestAINodePosition(enemy.isOutside ? RoundManager.Instance.outsideAINodes : RoundManager.Instance.insideAINodes, originalPosition);
            CreateInvocationObjectClientRpc(new NetworkObjectReference(enemy.gameObject), spawnPosition);
            yield return new WaitForSeconds(3.05f);
            if (!StartOfRound.Instance.inShipPhase)
            {
                if (Compatibility.BloodMoonSpawnEnemySpecific(enemy, spawnPosition))
                    yield break;
                else RoundManager.Instance.SpawnEnemyGameObject(spawnPosition, Random.Range(-90f, 90f), -1, enemy.enemyType);
            }
        }

        [ClientRpc]
        private void CreateInvocationObjectClientRpc(NetworkObjectReference enemyRef, Vector3 position)
        {
            StartCoroutine(CreateInvocationObject(enemyRef, position));
        }

        private IEnumerator CreateInvocationObject(NetworkObjectReference enemyRef, Vector3 position)
        {
            var enemyObject = (GameObject)enemyRef;
            if (resurrectEnemyInvocationObject == null || resurrectEnemyBurstObject == null || enemyObject == null)
                yield break;
            var burstObject = Instantiate(resurrectEnemyBurstObject, position + (Vector3.up * 0.9f), Quaternion.identity);
            var invocationObject = Instantiate(resurrectEnemyInvocationObject, position, Quaternion.identity);
            yield return Effects.AnimateScaleDownObject(enemyObject, 3f);
            burstObject.GetComponent<AudioSource>().Play();
            yield return new WaitForSeconds(0.1f);
            if (invocationObject != null)
                Destroy(invocationObject);
            yield return new WaitForSeconds(2.4f);
            if (burstObject != null)
                Destroy(burstObject);
            if (IsServer && enemyObject != null)
            {
                var netObj = enemyObject.GetComponentInChildren<NetworkObject>();
                if (netObj != null && netObj.IsSpawned)
                    netObj.Despawn();
            }
        }

        [ServerRpc]
        public void InitializeManagerServerRpc(NetworkObjectReference managerReference)
        {
            InitializeManagerClientRpc(managerReference);
        }

        [ClientRpc]
        private void InitializeManagerClientRpc(NetworkObjectReference managerReference)
        {
            StartCoroutine(InitializeManager(managerReference));
        }

        private IEnumerator InitializeManager(NetworkObjectReference managerReference)
        {
            NetworkObject? netObj = null;
            float startTime = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup - startTime < 8f && !managerReference.TryGet(out netObj))
            {
                yield return new WaitForSeconds(0.03f);
            }
            if (netObj == null)
            {
                Plugin.logger.LogError("Error while trying to sync Blood Moon manager.");
                yield break;
            }
            yield return new WaitForEndOfFrame();
            localPlayer = GameNetworkManager.Instance.localPlayerController;
            if (localPlayer != null && !localPlayer.isPlayerDead)
                localPlayerIsInsideLastChecked = localPlayer.isInsideFactory;
            if (localPlayerIsInsideLastChecked)
                nextMoonSfxTime = moonSfxTimeIntervalInside;
            if (Plugin.config.bloodMoonSpecificEnemiesItemSpawningMode.Value == "Chance")
            {
                globalSeedForSpecificEnemyItemSpawning = new System.Random(StartOfRound.Instance.randomMapSeed);
                nextRandomSpecificEnemyItemSpawnChance = globalSeedForSpecificEnemyItemSpawning.Next(0, 100);
            }
            StartInitialEffects();
            isInitialized = true;
        }

        private void StartInitialEffects()
        {
            if (bloodMoonVolumeObject != null)
            {
                spawnedBloodVolume = Instantiate(bloodMoonVolumeObject);
                if (spawnedBloodVolume != null)
                {
                    if (Effects.SetupCustomSkyVolume(spawnedBloodVolume, out _))
                        volumeCorrectlyRendered = true;
                }
                else
                    Plugin.logger.LogError("Failed to instantiate Blood Moon Volume.");
            }
            if (localPlayer != null && bloodMoonParticlesObject != null)
            {
                var player = !localPlayer.isPlayerDead ? localPlayer : localPlayer.spectatedPlayerScript;
                spawnedBloodParticles = Instantiate(bloodMoonParticlesObject, player.transform.position + bloodParticleOffset, Quaternion.identity);
                if (spawnedBloodParticles == null)
                    Plugin.logger.LogError("Failed to instantiate Blood Moon Particles for player " + player.playerUsername + ".");
            }
            Effects.EnableVanillaVolumeFog(false, ref vanillaFogVolumeComponentExists);
            Effects.SetupWindSpeedComponents(ref visualEnvironments, ref originalWindSpeeds);
            Effects.IncreaseWindSpeed(visualEnvironments, windSpeedFactor);
            introMusicAudio.volume = Plugin.config.bloodMoonIntroMusicVolume.Value;
            introMusicAudio.Play();
        }

        // Perform a safety check to ensure the volume is correctly rendered
        private void PerformRenderingSafeCheck()
        {
            if (!volumeCorrectlyRendered && spawnedBloodVolume != null)
            {
                volumeRenderingSafeCheckTimer += Time.deltaTime;
                if (volumeRenderingSafeCheckTimer > 1f)
                {
                    if (Effects.SetupCustomSkyVolume(spawnedBloodVolume, out _))
                    {
                        volumeCorrectlyRendered = true;
                        volumeRenderingSafeCheckTimer = 0f;
                    }
                    else
                        Plugin.logger.LogError("Failed to setup fog state for the Blood Moon Volume.");
                }
            }
        }

        internal static bool UpdateFirstEnemyRespawned(string enemyName, bool? overrideValue = null)
        {
            if (firstEnemiesRespawned.ContainsKey(enemyName))
            {
                if (overrideValue.HasValue)
                {
                    firstEnemiesRespawned[enemyName] = overrideValue.Value;
                }
                return firstEnemiesRespawned[enemyName];
            }
            firstEnemiesRespawned[enemyName] = overrideValue ?? false;
            return false;
        }

        public override void OnDestroy()
        {
            if (introMusicAudio.isPlaying)
                introMusicAudio.Stop();
            if (sfxAudio.isPlaying)
                sfxAudio.Stop();
            if (spawnedBloodParticles != null)
                Destroy(spawnedBloodParticles);
            if (spawnedBloodVolume != null)
                Destroy(spawnedBloodVolume);
            Effects.EnableVanillaVolumeFog(true, ref vanillaFogVolumeComponentExists);
            for (int i = 0; i < visualEnvironments.Count; i++)
            {
                if (visualEnvironments[i] != null)
                    visualEnvironments[i].windSpeed.value = originalWindSpeeds[i];
            }
            visualEnvironments.Clear();
            originalWindSpeeds.Clear();
            firstEnemiesRespawned.Clear();
            base.OnDestroy();
        }
    }
}
