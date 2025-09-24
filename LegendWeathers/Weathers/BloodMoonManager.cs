using GameNetcodeStuff;
using LegendWeathers.Utils;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace LegendWeathers.Weathers
{
    public class BloodMoonManager : NetworkBehaviour
    {
        public AudioSource introMusicAudio = null!;
        public AudioSource sfxAudio = null!;
        public AudioClip[] sfx = null!;
        public GameObject resurrectEnemyInvocationObject = null!;
        public GameObject resurrectEnemyBurstObject = null!;

        private bool isInitialized = false;
        private PlayerControllerB? localPlayer = null;
        private bool localPlayerIsInsideLastChecked = false;

        private float lastMoonSfxTime = 0;
        private float nextMoonSfxTime = 63;  // 63 = initial delay outside before first sfx
        private readonly float moonSfxTimeIntervalInside = 10.8f;
        private readonly float moonSfxTimeIntervalOutside = 81.8f;

        private float lastChanceEventTime = 0;
        private readonly float nextChanceEventTime = 60;

        public void Update()
        {
            if (!isInitialized)
                return;
            Effects.StopVanillaMusic();
            TryAndPlayChanceEvent();
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
                sfxAudio.PlayOneShot(sfx[Random.Range(0, sfx.Length)], 0.5f);
            }
        }

        private void TryAndPlayChanceEvent()
        {
            if (!IsServer)
                return;
            lastChanceEventTime += Time.deltaTime;
            if (lastChanceEventTime >= nextChanceEventTime)
            {
                lastChanceEventTime = 0;
                if (Random.Range(0, 10) == 0)
                    SpawnBloodStone();
            }
        }

        private void SpawnBloodStone(Vector3? position = null)
        {
            Vector3 spawnPosition = position == null ? Effects.GetRandomMoonPosition() : RoundManager.Instance.GetRandomNavMeshPositionInRadius((Vector3)position, 5);
            // spawning effect here
        }

        public void ResurrectEnemy(EnemyAI enemy)
        {
            if (IsServer)
                StartCoroutine(StartAnimationThenResurrect(enemy, enemy.transform.position));
        }

        private IEnumerator StartAnimationThenResurrect(EnemyAI enemy, Vector3 originalPosition)
        {
            SpawnBloodStone(originalPosition);
            yield return new WaitForSeconds(Plugin.config.bloodMoonResurrectWaitTime.Value);
            if (StartOfRound.Instance.inShipPhase || StartOfRound.Instance.shipIsLeaving)
                yield break;
            var spawnPosition = RoundManager.Instance.GetNavMeshPosition(originalPosition, sampleRadius: 3f);
            if (!RoundManager.Instance.GotNavMeshPositionResult)
                spawnPosition = Effects.GetClosestAINodePosition(enemy.isOutside ? RoundManager.Instance.outsideAINodes : RoundManager.Instance.insideAINodes, originalPosition);
            CreateInvocationObjectClientRpc(new NetworkObjectReference(enemy.gameObject), spawnPosition);
            yield return new WaitForSeconds(3.05f);
            if (!StartOfRound.Instance.inShipPhase)
            {
                RoundManager.Instance.SpawnEnemyGameObject(spawnPosition, Random.Range(-90f, 90f), -1, enemy.enemyType);
            }
        }

        [ClientRpc]
        private void CreateInvocationObjectClientRpc(NetworkObjectReference enemyRef, Vector3 position)
        {
            StartCoroutine(CreateInvocationObject(enemyRef, position));
        }

        private IEnumerator CreateInvocationObject(NetworkObjectReference enemyRef, Vector3 position)
        {
            if (resurrectEnemyInvocationObject == null || resurrectEnemyBurstObject == null)
                yield break;
            var enemyObject = (GameObject)enemyRef;
            var invocationObject = Instantiate(resurrectEnemyInvocationObject, position, Quaternion.identity);
            yield return Effects.AnimateScaleDownObject(enemyObject, 3f);
            Instantiate(resurrectEnemyBurstObject, position + (Vector3.up * 0.9f), Quaternion.identity);
            yield return new WaitForSeconds(0.1f);
            if (invocationObject != null)
                Destroy(invocationObject);
            if (IsServer)
                enemyObject.GetComponentInChildren<NetworkObject>().Despawn();
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
            StartIntroductionMusic();
            isInitialized = true;
        }

        private void StartIntroductionMusic()
        {
            introMusicAudio.volume = Plugin.config.bloodMoonMusicVolume.Value;
            introMusicAudio.Play();
        }

        public override void OnDestroy()
        {
            if (introMusicAudio.isPlaying)
                introMusicAudio.Stop();
            if (sfxAudio.isPlaying)
                sfxAudio.Stop();
            base.OnDestroy();
        }
    }
}
